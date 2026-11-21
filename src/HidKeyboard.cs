using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlueforceRGB
{
    public class HidKeyboard : IDisposable
    {
        private const ushort TARGET_VID = 0x5566;
        private const ushort TARGET_PID = 0x0008;

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            public ushort InputReportByteLength;
            public ushort OutputReportByteLength;
            public ushort FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public ushort[] Reserved;
            public ushort NumberLinkCollectionNodes;
            public ushort NumberInputButtonCaps;
            public ushort NumberInputValueCaps;
            public ushort NumberInputDataIndices;
            public ushort NumberOutputButtonCaps;
            public ushort NumberOutputValueCaps;
            public ushort NumberOutputDataIndices;
            public ushort NumberFeatureButtonCaps;
            public ushort NumberFeatureValueCaps;
            public ushort NumberFeatureDataIndices;
        }

        [DllImport("hid.dll")] private static extern void HidD_GetHidGuid(out Guid hidGuid);
        [DllImport("hid.dll")] private static extern bool HidD_GetPreparsedData(IntPtr h, out IntPtr pp);
        [DllImport("hid.dll")] private static extern bool HidD_FreePreparsedData(IntPtr pp);
        [DllImport("hid.dll")] private static extern bool HidD_FlushQueue(IntPtr h);
        [DllImport("hid.dll")] private static extern int HidP_GetCaps(IntPtr pp, out HIDP_CAPS caps);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(ref Guid g, IntPtr e, IntPtr p, uint f);
        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInterfaces(IntPtr info, IntPtr d, ref Guid g, uint idx, ref SP_DEVICE_INTERFACE_DATA ifd);
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr info, ref SP_DEVICE_INTERFACE_DATA ifd, IntPtr det, uint sz, out uint req, IntPtr d);
        [DllImport("setupapi.dll")]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr info);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(string fn, uint acc, uint sh, IntPtr sec, uint cr, uint fl, IntPtr t);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(IntPtr h, byte[] b, uint n, out uint w, IntPtr o);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(IntPtr h, byte[] b, uint n, out uint r, IntPtr o);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr h);

        private IntPtr _handle = new IntPtr(-1);
        private readonly object _lock = new object();

        public bool IsConnected { get { return _handle != new IntPtr(-1); } }
        public string DevicePath { get; private set; }
        public string LastError { get; private set; }

        public bool Connect()
        {
            lock (_lock)
            {
                DisconnectInternal();
                string path = FindDevicePath();
                if (path == null)
                {
                    LastError = "Blueforce / ZXW (5566:0008) klavye bulunamadı.";
                    return false;
                }

                IntPtr h = CreateFile(path, 0xC0000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
                if (h == new IntPtr(-1))
                {
                    LastError = "Klavye açılamadı (Win32 hata: " + Marshal.GetLastWin32Error() + ").";
                    return false;
                }

                _handle = h;
                DevicePath = path;
                LastError = null;
                HidD_FlushQueue(_handle);
                return true;
            }
        }

        public void Disconnect()
        {
            lock (_lock)
            {
                DisconnectInternal();
            }
        }

        private void DisconnectInternal()
        {
            if (_handle != new IntPtr(-1))
            {
                CloseHandle(_handle);
                _handle = new IntPtr(-1);
            }
        }

        private static string FindDevicePath()
        {
            Guid g;
            HidD_GetHidGuid(out g);
            IntPtr info = SetupDiGetClassDevs(ref g, IntPtr.Zero, IntPtr.Zero, 0x12);
            if (info == new IntPtr(-1)) return null;

            string target = string.Format("vid_{0:x4}&pid_{1:x4}", TARGET_VID, TARGET_PID);
            uint idx = 0;
            var ifData = new SP_DEVICE_INTERFACE_DATA { cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA)) };
            string found = null;

            try
            {
                while (SetupDiEnumDeviceInterfaces(info, IntPtr.Zero, ref g, idx++, ref ifData))
                {
                    uint req = 0;
                    SetupDiGetDeviceInterfaceDetail(info, ref ifData, IntPtr.Zero, 0, out req, IntPtr.Zero);
                    if (req == 0) continue;

                    IntPtr buf = Marshal.AllocHGlobal((int)req);
                    try
                    {
                        Marshal.WriteInt32(buf, IntPtr.Size == 8 ? 8 : 6);
                        if (SetupDiGetDeviceInterfaceDetail(info, ref ifData, buf, req, out req, IntPtr.Zero))
                        {
                            string p = Marshal.PtrToStringAuto(IntPtr.Add(buf, 4));
                            if (p == null || p.IndexOf(target, StringComparison.OrdinalIgnoreCase) < 0)
                                continue;

                            IntPtr h = CreateFile(p, 0, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
                            if (h != new IntPtr(-1))
                            {
                                try
                                {
                                    IntPtr pp;
                                    if (HidD_GetPreparsedData(h, out pp))
                                    {
                                        HIDP_CAPS caps;
                                        HidP_GetCaps(pp, out caps);
                                        HidD_FreePreparsedData(pp);
                                        if (caps.InputReportByteLength >= 65 && caps.OutputReportByteLength >= 65)
                                        {
                                            found = p;
                                        }
                                    }
                                }
                                finally { CloseHandle(h); }
                                if (found != null) break;
                            }
                        }
                    }
                    finally { Marshal.FreeHGlobal(buf); }
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(info);
            }
            return found;
        }

        /// <summary>
        /// Sends a 65-byte HID command and strictly verifies both ACK (0xAA) and command byte match (resp[2] == pkt[2]).
        /// </summary>
        private bool SendRecvExact(byte[] pkt, out byte[] resp, bool flushFirst)
        {
            resp = new byte[65];
            if (_handle == new IntPtr(-1)) return false;

            byte sum = 0;
            for (int i = 5; i < 65; i++) sum += pkt[i];
            pkt[4] = sum;
            byte expectedCmd = pkt[2];

            for (int attempt = 0; attempt < 3; attempt++)
            {
                if (flushFirst || attempt > 0)
                    HidD_FlushQueue(_handle);

                uint w = 0, r = 0;
                if (!WriteFile(_handle, pkt, 65, out w, IntPtr.Zero))
                {
                    LastError = "HID yazma hatası: " + Marshal.GetLastWin32Error();
                    DisconnectInternal();
                    return false;
                }

                // Read up to 3 queued reports in case a stale packet was pending
                for (int drain = 0; drain < 3; drain++)
                {
                    if (!ReadFile(_handle, resp, 65, out r, IntPtr.Zero))
                    {
                        LastError = "HID okuma hatası: " + Marshal.GetLastWin32Error();
                        DisconnectInternal();
                        return false;
                    }
                    if (resp[1] == 0xAA && resp[2] == expectedCmd)
                        return true;
                }

                Thread.Sleep(15);
            }
            return false;
        }

        private bool SendBegin(bool flushFirst)
        {
            byte[] p = new byte[65];
            p[1] = 0x55; p[2] = 0x01;
            byte[] resp;
            return SendRecvExact(p, out resp, flushFirst);
        }

        private bool SendEnd()
        {
            byte[] p = new byte[65];
            p[1] = 0x55; p[2] = 0x02;
            byte[] resp;
            return SendRecvExact(p, out resp, false);
        }

        public byte[] ReadLightConfig()
        {
            lock (_lock)
            {
                if (!IsConnected) return null;
                if (!SendBegin(true)) return null;
                byte[] p5 = new byte[65];
                p5[1] = 0x55; p5[2] = 0x05; p5[5] = 0x20;
                byte[] resp;
                bool ok = SendRecvExact(p5, out resp, false);
                SendEnd();
                return ok ? resp : null;
            }
        }

        /// <summary>
        /// Writes a clean, verified 0x55 0x06 light configuration packet (never copies corrupted bytes).
        /// </summary>
        public bool SetBuiltInMode(byte modeId, byte brightness0to100, byte speed0to4, byte direction, bool colorful, byte r, byte g, byte b)
        {
            lock (_lock)
            {
                if (!IsConnected) return false;
                if (!SendBegin(true)) return false;

                byte[] pkt = new byte[65];
                pkt[0] = 0x00;
                pkt[1] = 0x55;
                pkt[2] = 0x06;
                pkt[3] = 0x00;
                pkt[5] = 0x20;
                pkt[9] = 0x02;
                pkt[10] = 0xAA;
                pkt[11] = modeId;
                pkt[12] = brightness0to100;
                pkt[13] = (byte)Math.Max(0, Math.Min(4, 4 - speed0to4));
                pkt[14] = direction;
                pkt[15] = (byte)(colorful ? 1 : 0);
                pkt[16] = 0x00;
                pkt[17] = r;
                pkt[18] = g;
                pkt[19] = b;
                pkt[24] = 0x01;
                pkt[25] = 0x04;

                byte[] resp;
                bool ok = SendRecvExact(pkt, out resp, false);
                SendEnd();
                return ok;
            }
        }

        /// <summary>
        /// Fast, smooth 50+ FPS solid color update via Mod 0x03 (no HidD_FlushQueue stall per frame).
        /// </summary>
        public bool SetLiveSolidColor(byte brightness0to100, byte r, byte g, byte b)
        {
            lock (_lock)
            {
                if (!IsConnected) return false;
                if (!SendBegin(false)) return false;

                byte[] pkt = new byte[65];
                pkt[0] = 0x00;
                pkt[1] = 0x55;
                pkt[2] = 0x06;
                pkt[3] = 0x00;
                pkt[5] = 0x20;
                pkt[9] = 0x02;
                pkt[10] = 0xAA;
                pkt[11] = 0x03;
                pkt[12] = brightness0to100;
                pkt[13] = 0x02;
                pkt[14] = 0x00;
                pkt[15] = 0x00;
                pkt[16] = 0x00;
                pkt[17] = r;
                pkt[18] = g;
                pkt[19] = b;
                pkt[24] = 0x01;
                pkt[25] = 0x04;

                byte[] resp;
                bool ok = SendRecvExact(pkt, out resp, false);
                SendEnd();
                return ok;
            }
        }

        public bool EnableLiveMode()
        {
            return SetBuiltInMode(0x2E, 100, 2, 0, true, 255, 255, 255);
        }

        public bool SendLiveFrame(byte[] rgb384)
        {
            return SendPerKeyBuffer(0xDD, rgb384);
        }

        public bool SaveCustomPerKeyFrame(byte[] rgb384)
        {
            return SendPerKeyBuffer(0x0B, rgb384);
        }

        private bool SendPerKeyBuffer(byte cmd, byte[] rgb384)
        {
            if (rgb384 == null || rgb384.Length < 384) return false;
            lock (_lock)
            {
                if (!IsConnected) return false;
                if (!SendBegin(false)) return false;

                byte[] resp;
                for (int i = 0; i < 7; i++)
                {
                    byte[] pkt = new byte[65];
                    pkt[0] = 0x00;
                    pkt[1] = 0x55;
                    pkt[2] = cmd;
                    int len = (i == 6) ? 48 : 56;
                    int off = i * 56;
                    pkt[5] = (byte)len;
                    pkt[6] = (byte)(off & 0xFF);
                    pkt[7] = (byte)((off >> 8) & 0xFF);
                    Array.Copy(rgb384, off, pkt, 9, len);

                    if (!SendRecvExact(pkt, out resp, false))
                    {
                        SendEnd();
                        return false;
                    }
                }

                return SendEnd();
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
