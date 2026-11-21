using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BlueforceRGB
{
    public class MainForm : Form
    {
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private readonly HidKeyboard _keyboard = new HidKeyboard();

        private Label _statusLabel;
        private Label _activeModeLabel;
        private Label _lblBrightVal;
        private Label _lblSpeedVal;
        private ModernSlider _sliderBright;
        private ModernSlider _sliderSpeed;
        private ModernPillButton _btnDirRight;
        private ModernPillButton _btnDirLeft;
        private ModernToggle _toggleColorful;
        private ModernPillButton _btnCustomColor;
        private FlowLayoutPanel _cardsFlow;
        private readonly List<ModernModeCard> _allCards = new List<ModernModeCard>();
        private readonly List<ModernPillButton> _catButtons = new List<ModernPillButton>();
        private readonly List<ModernColorDot> _colorDots = new List<ModernColorDot>();
        private NotifyIcon _trayIcon;

        private byte _currentMode = 0x09;
        private int _brightness = 100;
        private int _speed = 2; // 0..4
        private int _direction = 0; // 0:Right, 1:Left
        private bool _colorful = true;
        private Color _selectedColor = Color.FromArgb(0, 229, 255);
        private bool _suppressApply = false;
        private string _activeCategory = "Tümü";

        private class ModeDef
        {
            public byte Id;
            public string Name, Desc, Cat;
            public Color Accent;
        }

        private static readonly ModeDef[] VerifiedModes = new[]
        {
            new ModeDef { Id = 0x09, Name = "Çapraz Gökkuşağı",     Desc = "60 FPS çapraz akış",      Cat = "Dalga",  Accent = Color.FromArgb(0, 225, 255) },
            new ModeDef { Id = 0x0A, Name = "Gökkuşağı Girdabı",     Desc = "Merkezden dönen spiral",  Cat = "Dalga",  Accent = Color.FromArgb(140, 100, 255) },
            new ModeDef { Id = 0x16, Name = "Süzülen Dalga",         Desc = "Yumuşak yatay akış",      Cat = "Dalga",  Accent = Color.FromArgb(0, 240, 195) },
            new ModeDef { Id = 0x07, Name = "Dikey Dalga",           Desc = "Yukarıdan aşağıya geçiş", Cat = "Dalga",  Accent = Color.FromArgb(50, 180, 255) },
            new ModeDef { Id = 0x08, Name = "Fıskiye / Çiçek",       Desc = "Ortadan fıskiye yayılım", Cat = "Dalga",  Accent = Color.FromArgb(255, 80, 185) },
            new ModeDef { Id = 0x02, Name = "Mavi-Pembe Geçiş",      Desc = "Sol-sağ renk geçişi",     Cat = "Dalga",  Accent = Color.FromArgb(215, 70, 255) },
            new ModeDef { Id = 0x0B, Name = "Gelgit Tarama",         Desc = "Soldan sağa sönen akış",  Cat = "Dalga",  Accent = Color.FromArgb(0, 195, 255) },
            new ModeDef { Id = 0x0C, Name = "Kayan Gökkuşağı",       Desc = "Gidip gelen parlak şerit",Cat = "Dalga",  Accent = Color.FromArgb(255, 130, 60) },
            new ModeDef { Id = 0x12, Name = "Gökkuşağı Yılanı",      Desc = "Satır satır gezen yılan", Cat = "Dalga",  Accent = Color.FromArgb(90, 255, 130) },
            new ModeDef { Id = 0x06, Name = "Işıltılı Akış",         Desc = "Akıcı donanım efekti",    Cat = "Dalga",  Accent = Color.FromArgb(0, 220, 255) },

            new ModeDef { Id = 0x0D, Name = "Tuş Dalgası (Ripple)",  Desc = "Bastığın tuştan yayılır", Cat = "Tepki",  Accent = Color.FromArgb(255, 55, 155) },
            new ModeDef { Id = 0x0E, Name = "Renk Değiştiren Dalga", Desc = "Her tuşta renk değişir",  Cat = "Tepki",  Accent = Color.FromArgb(230, 80, 255) },
            new ModeDef { Id = 0x0F, Name = "Tek Tuş İz",            Desc = "Bastığın tuş yanıp söner",Cat = "Tepki",  Accent = Color.FromArgb(255, 100, 210) },
            new ModeDef { Id = 0x10, Name = "Yatay Lazer",           Desc = "Bastığın satırda lazer",  Cat = "Tepki",  Accent = Color.FromArgb(255, 65, 115) },
            new ModeDef { Id = 0x11, Name = "Artı Havai Fişek",      Desc = "Bastığın tuşta artı",     Cat = "Tepki",  Accent = Color.FromArgb(255, 150, 60) },

            new ModeDef { Id = 0x01, Name = "Spektrum Döngü",        Desc = "Tüm renkler sırayla",     Cat = "Ortam",  Accent = Color.FromArgb(115, 225, 255) },
            new ModeDef { Id = 0x04, Name = "Nefes Alma",            Desc = "Renkli yanıp sönme",      Cat = "Ortam",  Accent = Color.FromArgb(0, 245, 165) },
            new ModeDef { Id = 0x05, Name = "Gökkuşağı Parıltısı",   Desc = "Dolu renkli ışıltılar",   Cat = "Ortam",  Accent = Color.FromArgb(255, 200, 65) },
            new ModeDef { Id = 0x13, Name = "Yıldızlı Gece",         Desc = "Parlayıp sönen yıldızlar",Cat = "Ortam",  Accent = Color.FromArgb(145, 185, 255) },
            new ModeDef { Id = 0x14, Name = "Renkli Yağmur",         Desc = "Sütunlardan akan damla",  Cat = "Ortam",  Accent = Color.FromArgb(0, 255, 120) },
            new ModeDef { Id = 0x15, Name = "Otomatik Havai Fişek",  Desc = "Rastgele renkli patlama", Cat = "Ortam",  Accent = Color.FromArgb(255, 175, 50) },
            new ModeDef { Id = 0x03, Name = "Sabit Renk",            Desc = "Seçtiğin tek RGB renk",   Cat = "Ortam",  Accent = Color.FromArgb(240, 245, 255) },
            new ModeDef { Id = 0x00, Name = "Işıkları Kapat",        Desc = "Tüm LED'leri kapatır",    Cat = "Ortam",  Accent = Color.FromArgb(115, 125, 150) }
        };

        private string SettingsPath
        {
            get
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlueforceRGB");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return Path.Combine(dir, "settings_final.ini");
            }
        }

        public MainForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;
            Text = "Blueforce RGB Studio";
            Size = new Size(1060, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(11, 13, 21);
            ForeColor = Color.FromArgb(235, 240, 255);
            Font = new Font("Segoe UI", 9.5f);

            LoadSettings();
            BuildUI();
            SetupTray();
            ConnectKeyboard(true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Top 2px neon gradient accent strip
            using (LinearGradientBrush strip = new LinearGradientBrush(new Rectangle(0, 0, Width, 3), Color.FromArgb(0, 230, 255), Color.FromArgb(185, 65, 255), 0f))
                g.FillRectangle(strip, 0, 0, Width, 3);

            // Subtle outer frame border
            using (Pen p = new Pen(Color.FromArgb(40, 48, 74), 1f))
                g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
        }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        private void BuildUI()
        {
            // Custom Top Title Bar
            Panel titleBar = new Panel
            {
                Location = new Point(1, 3),
                Size = new Size(Width - 2, 58),
                BackColor = Color.FromArgb(15, 18, 29),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            titleBar.MouseDown += DragWindow;

            Label lblBrand = new Label
            {
                Text = "BLUEFORCE",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(22, 16)
            };
            lblBrand.MouseDown += DragWindow;

            Label lblSub = new Label
            {
                Text = "RGB STUDIO",
                Font = new Font("Segoe UI Semibold", 13.5f),
                ForeColor = Color.FromArgb(0, 225, 255),
                AutoSize = true,
                Location = new Point(140, 16)
            };
            lblSub.MouseDown += DragWindow;

            _statusLabel = new Label
            {
                Text = "● Klavye aranıyor...",
                Font = new Font("Segoe UI Semibold", 9.0f),
                ForeColor = Color.FromArgb(255, 200, 80),
                AutoSize = true,
                Location = new Point(285, 20)
            };
            _statusLabel.MouseDown += DragWindow;

            ModernPillButton btnReconnect = new ModernPillButton { Text = "↻ Bağlan", Size = new Size(92, 30), Location = new Point(Width - 270, 14) };
            btnReconnect.Click += (s, e) => ConnectKeyboard(true);

            ModernPillButton btnTray = new ModernPillButton { Text = "Tepsi", Size = new Size(64, 30), Location = new Point(Width - 170, 14) };
            btnTray.Click += (s, e) => MinimizeToTray();

            ModernPillButton btnMin = new ModernPillButton { Text = "—", Size = new Size(34, 30), Location = new Point(Width - 98, 14) };
            btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;

            ModernPillButton btnClose = new ModernPillButton { Text = "✕", Size = new Size(34, 30), Location = new Point(Width - 56, 14), IdleBack = Color.FromArgb(42, 24, 38) };
            btnClose.Click += (s, e) => Close();

            titleBar.Controls.AddRange(new Control[] { lblBrand, lblSub, _statusLabel, btnReconnect, btnTray, btnMin, btnClose });
            Controls.Add(titleBar);

            // Right Control Card (Glass panel)
            Panel rightCard = new Panel
            {
                Location = new Point(Width - 322, 76),
                Size = new Size(300, Height - 96),
                BackColor = Color.FromArgb(16, 19, 31)
            };
            rightCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = GfxUtil.RoundedRect(new RectangleF(0.5f, 0.5f, rightCard.Width - 1, rightCard.Height - 1), 14f))
                using (Pen pen = new Pen(Color.FromArgb(38, 46, 72), 1f))
                    e.Graphics.DrawPath(pen, gp);
            };
            Controls.Add(rightCard);

            int ry = 18;
            rightCard.Controls.Add(MakeCaption("SEÇİLİ 60 FPS MOD", 20, ry));
            ry += 20;

            _activeModeLabel = new Label
            {
                Text = GetModeName(_currentMode),
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 230, 255),
                Location = new Point(19, ry),
                Size = new Size(262, 28)
            };
            rightCard.Controls.Add(_activeModeLabel);
            ry += 42;

            // Brightness
            _lblBrightVal = new Label { Text = "Parlaklık   •   " + _brightness + "%", Location = new Point(20, ry), AutoSize = true, Font = new Font("Segoe UI Semibold", 9.2f), ForeColor = Color.FromArgb(210, 220, 245) };
            rightCard.Controls.Add(_lblBrightVal);
            ry += 22;
            _sliderBright = new ModernSlider { Minimum = 0, Maximum = 100, Value = _brightness, Location = new Point(16, ry), Width = 268 };
            _sliderBright.ValueChanged += (s, e) =>
            {
                _brightness = _sliderBright.Value;
                _lblBrightVal.Text = "Parlaklık   •   " + _brightness + "%";
            };
            _sliderBright.DragCompleted += (s, e) => ApplyToKeyboard();
            rightCard.Controls.Add(_sliderBright);
            ry += 40;

            // Speed
            string[] speedNames = { "Çok Yavaş", "Yavaş", "Orta", "Hızlı", "Çok Hızlı" };
            _lblSpeedVal = new Label { Text = "Hız   •   " + speedNames[_speed], Location = new Point(20, ry), AutoSize = true, Font = new Font("Segoe UI Semibold", 9.2f), ForeColor = Color.FromArgb(210, 220, 245) };
            rightCard.Controls.Add(_lblSpeedVal);
            ry += 22;
            _sliderSpeed = new ModernSlider { Minimum = 0, Maximum = 4, Value = _speed, Location = new Point(16, ry), Width = 268 };
            _sliderSpeed.ValueChanged += (s, e) =>
            {
                _speed = _sliderSpeed.Value;
                _lblSpeedVal.Text = "Hız   •   " + speedNames[_speed];
            };
            _sliderSpeed.DragCompleted += (s, e) => ApplyToKeyboard();
            rightCard.Controls.Add(_sliderSpeed);
            ry += 44;

            // Direction segmented pills
            rightCard.Controls.Add(MakeCaption("AKIŞ YÖNÜ", 20, ry));
            ry += 22;
            _btnDirRight = new ModernPillButton { Text = "Sağa →", Size = new Size(126, 32), Location = new Point(20, ry), Active = (_direction == 0) };
            _btnDirLeft  = new ModernPillButton { Text = "← Sola", Size = new Size(126, 32), Location = new Point(154, ry), Active = (_direction == 1) };
            _btnDirRight.Click += (s, e) => { _direction = 0; _btnDirRight.Active = true; _btnDirLeft.Active = false; ApplyToKeyboard(); };
            _btnDirLeft.Click  += (s, e) => { _direction = 1; _btnDirRight.Active = false; _btnDirLeft.Active = true; ApplyToKeyboard(); };
            rightCard.Controls.Add(_btnDirRight);
            rightCard.Controls.Add(_btnDirLeft);
            ry += 48;

            // Rainbow toggle row
            Label lblRain = new Label { Text = "Gökkuşağı Renkleri", Location = new Point(20, ry + 3), AutoSize = true, Font = new Font("Segoe UI Semibold", 9.5f), ForeColor = Color.FromArgb(220, 230, 250) };
            _toggleColorful = new ModernToggle { Checked = _colorful, Location = new Point(234, ry) };
            _toggleColorful.CheckedChanged += (s, e) =>
            {
                _colorful = _toggleColorful.Checked;
                if (!_suppressApply) ApplyToKeyboard();
            };
            rightCard.Controls.Add(lblRain);
            rightCard.Controls.Add(_toggleColorful);
            ry += 44;

            // Color swatches
            rightCard.Controls.Add(MakeCaption("TEK RENK PALETİ", 20, ry));
            ry += 24;

            Color[] swatches = new[]
            {
                Color.FromArgb(255, 25, 45),
                Color.FromArgb(255, 95, 0),
                Color.FromArgb(255, 210, 0),
                Color.FromArgb(0, 240, 95),
                Color.FromArgb(0, 225, 255),
                Color.FromArgb(30, 105, 255),
                Color.FromArgb(145, 55, 255),
                Color.FromArgb(255, 40, 165),
                Color.FromArgb(255, 125, 205),
                Color.FromArgb(115, 255, 220),
                Color.FromArgb(255, 255, 255),
                Color.FromArgb(255, 215, 145)
            };
            for (int i = 0; i < swatches.Length; i++)
            {
                Color sw = swatches[i];
                ModernColorDot dot = new ModernColorDot
                {
                    SwatchColor = sw,
                    Selected = (!_colorful && sw.ToArgb() == _selectedColor.ToArgb()),
                    Location = new Point(20 + (i % 6) * 44, ry + (i / 6) * 40)
                };
                dot.Click += (s, e) =>
                {
                    _selectedColor = sw;
                    _suppressApply = true;
                    _toggleColorful.Checked = false;
                    _colorful = false;
                    _suppressApply = false;
                    RefreshColorDots();
                    ApplyToKeyboard();
                };
                _colorDots.Add(dot);
                rightCard.Controls.Add(dot);
            }
            ry += 92;

            _btnCustomColor = new ModernPillButton { Text = "Özel Renk Seç...", Size = new Size(260, 36), Location = new Point(20, ry), Active = false };
            _btnCustomColor.Click += (s, e) => PickColor();
            rightCard.Controls.Add(_btnCustomColor);
            ry += 50;

            Label lblFooter = new Label
            {
                Text = "Tüm 23 mod klavye çipinde 60 FPS çalışır ve uygulamayı kapatsan bile kalıcı olur.",
                Location = new Point(20, ry),
                Size = new Size(260, 44),
                Font = new Font("Segoe UI", 8.0f),
                ForeColor = Color.FromArgb(115, 128, 162)
            };
            rightCard.Controls.Add(lblFooter);

            // Left Section: Category Filter Pills + Smooth Mode Cards Grid
            string[][] cats = new[]
            {
                new[] { "Tümü",  "Tümü (23)" },
                new[] { "Dalga", "Dalga & Akış" },
                new[] { "Tepki", "Tuş Tepkili" },
                new[] { "Ortam", "Ortam & Sabit" }
            };
            int cx = 22;
            foreach (string[] c in cats)
            {
                string catKey = c[0];
                ModernPillButton pb = new ModernPillButton
                {
                    Text = c[1],
                    Tag = catKey,
                    Size = new Size(122, 32),
                    Location = new Point(cx, 76),
                    Active = (catKey == "Tümü")
                };
                pb.Click += (s, e) => FilterCategory(catKey);
                _catButtons.Add(pb);
                Controls.Add(pb);
                cx += 130;
            }

            _cardsFlow = new FlowLayoutPanel
            {
                Location = new Point(20, 120),
                Size = new Size(Width - 356, Height - 140),
                BackColor = Color.FromArgb(11, 13, 21),
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            Controls.Add(_cardsFlow);

            foreach (ModeDef m in VerifiedModes)
            {
                ModernModeCard card = new ModernModeCard
                {
                    ModeId = m.Id,
                    Title = m.Name,
                    Subtitle = m.Desc,
                    Category = m.Cat,
                    AccentColor = m.Accent,
                    Size = new Size(218, 60),
                    Margin = new Padding(4),
                    Selected = (m.Id == _currentMode)
                };
                card.Click += (s, e) => SelectMode(((ModernModeCard)s).ModeId);
                _allCards.Add(card);
            }

            FilterCategory("Tümü");
        }

        private void FilterCategory(string cat)
        {
            _activeCategory = cat;
            foreach (ModernPillButton b in _catButtons)
                b.Active = ((string)b.Tag == cat);

            _cardsFlow.SuspendLayout();
            _cardsFlow.Controls.Clear();
            foreach (ModernModeCard c in _allCards)
            {
                if (cat == "Tümü" || c.Category == cat)
                    _cardsFlow.Controls.Add(c);
            }
            _cardsFlow.ResumeLayout();
        }

        private void SelectMode(byte modeId)
        {
            _currentMode = modeId;
            _activeModeLabel.Text = GetModeName(modeId);
            if (modeId == 0x03)
            {
                _suppressApply = true;
                _toggleColorful.Checked = false;
                _colorful = false;
                _suppressApply = false;
            }
            foreach (ModernModeCard c in _allCards)
                c.Selected = (c.ModeId == _currentMode);
            ApplyToKeyboard();
        }

        private void PickColor()
        {
            using (ColorDialog cd = new ColorDialog { FullOpen = true, Color = _selectedColor })
            {
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    _selectedColor = cd.Color;
                    _suppressApply = true;
                    _toggleColorful.Checked = false;
                    _colorful = false;
                    _suppressApply = false;
                    RefreshColorDots();
                    ApplyToKeyboard();
                }
            }
        }

        private void RefreshColorDots()
        {
            foreach (ModernColorDot d in _colorDots)
            {
                d.Selected = (!_colorful && d.SwatchColor.ToArgb() == _selectedColor.ToArgb());
                d.Invalidate();
            }
        }

        private void ConnectKeyboard(bool applyCurrent)
        {
            if (_keyboard.Connect())
            {
                _statusLabel.Text = "● Bağlı  •  Blueforce 60 FPS";
                _statusLabel.ForeColor = Color.FromArgb(0, 238, 155);
                if (applyCurrent) ApplyToKeyboard();
            }
            else
            {
                _statusLabel.Text = "● Klavye bulunamadı";
                _statusLabel.ForeColor = Color.FromArgb(255, 95, 110);
            }
        }

        private void ApplyToKeyboard()
        {
            SaveSettings();
            if (!_keyboard.IsConnected)
            {
                if (!_keyboard.Connect()) return;
            }
            bool ok = _keyboard.SetBuiltInMode(
                _currentMode,
                (byte)_brightness,
                (byte)_speed,
                (byte)_direction,
                _colorful,
                _selectedColor.R,
                _selectedColor.G,
                _selectedColor.B
            );
            if (ok)
            {
                _statusLabel.Text = "● Bağlı  •  " + GetModeName(_currentMode);
                _statusLabel.ForeColor = Color.FromArgb(0, 238, 155);
            }
            else
            {
                _statusLabel.Text = "● Bağlantı hatası — Yeniden Bağlan";
                _statusLabel.ForeColor = Color.FromArgb(255, 95, 110);
            }
        }

        private static string GetModeName(byte modeId)
        {
            foreach (ModeDef m in VerifiedModes)
                if (m.Id == modeId) return m.Name;
            return "Mod 0x" + modeId.ToString("X2");
        }

        private void SetupTray()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Aç", null, (s, e) => RestoreFromTray());
            menu.Items.Add("Çıkış", null, (s, e) => Close());

            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Blueforce RGB Studio",
                ContextMenuStrip = menu,
                Visible = true
            };
            _trayIcon.DoubleClick += (s, e) => RestoreFromTray();
        }

        private void MinimizeToTray()
        {
            Hide();
            _trayIcon.ShowBalloonTip(1500, "Blueforce RGB Studio", "Sistem tepsisine küçültüldü.", ToolTipIcon.Info);
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_trayIcon != null) { _trayIcon.Visible = false; _trayIcon.Dispose(); }
            _keyboard.Dispose();
            base.OnFormClosing(e);
        }

        private void SaveSettings()
        {
            try
            {
                File.WriteAllLines(SettingsPath, new[]
                {
                    "Mode=" + _currentMode,
                    "Brightness=" + _brightness,
                    "Speed=" + _speed,
                    "Direction=" + _direction,
                    "Colorful=" + (_colorful ? 1 : 0),
                    "Color=" + _selectedColor.ToArgb()
                });
            }
            catch { }
        }

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return;
                foreach (string line in File.ReadAllLines(SettingsPath))
                {
                    string[] kv = line.Split('=');
                    if (kv.Length != 2) continue;
                    int val;
                    if (!int.TryParse(kv[1], out val)) continue;
                    switch (kv[0])
                    {
                        case "Mode": _currentMode = (byte)Math.Max(0, Math.Min(0x16, val)); break;
                        case "Brightness": _brightness = Math.Max(0, Math.Min(100, val)); break;
                        case "Speed": _speed = Math.Max(0, Math.Min(4, val)); break;
                        case "Direction": _direction = val % 2; break;
                        case "Colorful": _colorful = (val != 0); break;
                        case "Color": _selectedColor = Color.FromArgb(val); break;
                    }
                }
            }
            catch { }
        }

        private static Label MakeCaption(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 8.0f),
                ForeColor = Color.FromArgb(115, 130, 165)
            };
        }
    }
}
