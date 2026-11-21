using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace BlueforceRGB
{
    public static class GfxUtil
    {
        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            float d = radius * 2f;
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    public class ModernModeCard : Control
    {
        public byte ModeId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Category { get; set; }
        public Color AccentColor { get; set; }
        private bool _selected;
        private bool _hovered;

        public bool Selected
        {
            get { return _selected; }
            set { if (_selected != value) { _selected = value; Invalidate(); } }
        }

        public ModernModeCard()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            AccentColor = Color.FromArgb(0, 220, 255);
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            RectangleF rect = new RectangleF(1.5f, 1.5f, Width - 3f, Height - 3f);
            using (GraphicsPath path = GfxUtil.RoundedRect(rect, 11f))
            {
                Color topCol = _selected ? Color.FromArgb(26, 36, 58) : (_hovered ? Color.FromArgb(24, 29, 46) : Color.FromArgb(18, 21, 34));
                Color botCol = _selected ? Color.FromArgb(18, 24, 42) : (_hovered ? Color.FromArgb(19, 23, 38) : Color.FromArgb(15, 18, 29));
                using (LinearGradientBrush bg = new LinearGradientBrush(rect, topCol, botCol, 90f))
                    g.FillPath(bg, path);

                if (_selected)
                {
                    using (SolidBrush glow = new SolidBrush(Color.FromArgb(22, AccentColor)))
                        g.FillPath(glow, path);
                }

                Color border = _selected ? AccentColor : (_hovered ? Color.FromArgb(75, 88, 125) : Color.FromArgb(34, 40, 62));
                using (Pen pen = new Pen(border, _selected ? 1.6f : 1.0f))
                    g.DrawPath(pen, path);
            }

            // Neon vertical pill bar on the left edge
            RectangleF pill = new RectangleF(9f, 12f, 3.8f, Height - 24f);
            using (GraphicsPath pp = GfxUtil.RoundedRect(pill, 2f))
            using (SolidBrush pb = new SolidBrush(_selected ? AccentColor : Color.FromArgb(_hovered ? 180 : 95, AccentColor)))
                g.FillPath(pb, pp);

            // Title & subtitle
            using (Font fTitle = new Font("Segoe UI Semibold", 9.3f))
            using (Font fSub = new Font("Segoe UI", 7.8f))
            using (SolidBrush bTitle = new SolidBrush(_selected ? Color.White : Color.FromArgb(225, 232, 248)))
            using (SolidBrush bSub = new SolidBrush(_selected ? Color.FromArgb(165, 210, 235) : Color.FromArgb(120, 134, 168)))
            {
                g.DrawString(Title ?? "", fTitle, bTitle, new RectangleF(20f, 10f, Width - 26f, 22f));
                g.DrawString(Subtitle ?? "", fSub, bSub, new RectangleF(20f, 32f, Width - 26f, 20f));
            }
        }
    }

    public class ModernPillButton : Control
    {
        private bool _active;
        private bool _hovered;
        public Color ActiveBack { get; set; }
        public Color IdleBack { get; set; }

        public bool Active
        {
            get { return _active; }
            set { if (_active != value) { _active = value; Invalidate(); } }
        }

        public ModernPillButton()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            ActiveBack = Color.FromArgb(0, 170, 220);
            IdleBack = Color.FromArgb(24, 28, 44);
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            RectangleF r = new RectangleF(1f, 1f, Width - 2f, Height - 2f);
            float rad = Math.Min(Height / 2f - 1f, 16f);
            using (GraphicsPath p = GfxUtil.RoundedRect(r, rad))
            {
                if (_active)
                {
                    using (LinearGradientBrush b = new LinearGradientBrush(r, ActiveBack, Color.FromArgb(115, 70, 245), 0f))
                        g.FillPath(b, p);
                }
                else
                {
                    Color fill = _hovered ? Color.FromArgb(34, 40, 62) : IdleBack;
                    using (SolidBrush b = new SolidBrush(fill))
                        g.FillPath(b, p);
                    using (Pen pen = new Pen(_hovered ? Color.FromArgb(80, 95, 135) : Color.FromArgb(42, 48, 72), 1f))
                        g.DrawPath(pen, p);
                }
            }

            using (Font f = new Font("Segoe UI Semibold", 8.8f))
            using (SolidBrush tb = new SolidBrush(_active ? Color.White : Color.FromArgb(195, 208, 235)))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(Text, f, tb, r, sf);
            }
        }
    }

    public class ModernSlider : Control
    {
        private int _min = 0, _max = 100, _val = 100;
        private bool _dragging = false;

        public event EventHandler ValueChanged;
        public event EventHandler DragCompleted;

        public int Minimum { get { return _min; } set { _min = value; Invalidate(); } }
        public int Maximum { get { return _max; } set { _max = value; Invalidate(); } }
        public int Value
        {
            get { return _val; }
            set
            {
                int nv = Math.Max(_min, Math.Min(_max, value));
                if (_val != nv)
                {
                    _val = nv;
                    Invalidate();
                    if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
                }
            }
        }

        public ModernSlider()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Height = 28;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        private void UpdateFromX(int x)
        {
            float pad = 10f;
            float usable = Math.Max(1f, Width - pad * 2f);
            float t = Math.Max(0f, Math.Min(1f, (x - pad) / usable));
            Value = (int)Math.Round(_min + t * (_max - _min));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _dragging = true;
            UpdateFromX(e.X);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging) UpdateFromX(e.X);
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_dragging)
            {
                _dragging = false;
                if (DragCompleted != null) DragCompleted(this, EventArgs.Empty);
            }
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float pad = 10f;
            float cy = Height / 2f;
            float trackH = 6f;
            float usable = Width - pad * 2f;
            float t = _max > _min ? (float)(_val - _min) / (_max - _min) : 0f;
            float thumbX = pad + t * usable;

            // Track background
            RectangleF track = new RectangleF(pad, cy - trackH / 2f, usable, trackH);
            using (GraphicsPath tp = GfxUtil.RoundedRect(track, 3f))
            using (SolidBrush tb = new SolidBrush(Color.FromArgb(30, 36, 56)))
                g.FillPath(tb, tp);

            // Gradient fill
            if (thumbX > pad + 2f)
            {
                RectangleF fill = new RectangleF(pad, cy - trackH / 2f, thumbX - pad, trackH);
                using (GraphicsPath fp = GfxUtil.RoundedRect(fill, 3f))
                using (LinearGradientBrush fb = new LinearGradientBrush(track, Color.FromArgb(0, 225, 255), Color.FromArgb(165, 75, 255), 0f))
                    g.FillPath(fb, fp);
            }

            // Outer glow + circular thumb
            using (SolidBrush halo = new SolidBrush(Color.FromArgb(55, 0, 225, 255)))
                g.FillEllipse(halo, thumbX - 10f, cy - 10f, 20f, 20f);
            using (SolidBrush thumb = new SolidBrush(Color.White))
            using (Pen tp2 = new Pen(Color.FromArgb(0, 225, 255), 2f))
            {
                g.FillEllipse(thumb, thumbX - 6.5f, cy - 6.5f, 13f, 13f);
                g.DrawEllipse(tp2, thumbX - 6.5f, cy - 6.5f, 13f, 13f);
            }
        }
    }

    public class ModernToggle : Control
    {
        private bool _checked;
        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    Invalidate();
                    if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public ModernToggle()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Size = new Size(46, 24);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(1f, 1f, Width - 2f, Height - 2f);
            using (GraphicsPath p = GfxUtil.RoundedRect(r, (Height - 2f) / 2f))
            {
                if (_checked)
                {
                    using (LinearGradientBrush b = new LinearGradientBrush(r, Color.FromArgb(0, 210, 255), Color.FromArgb(165, 65, 255), 0f))
                        g.FillPath(b, p);
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(38, 44, 66)))
                        g.FillPath(b, p);
                }
            }
            float d = Height - 6f;
            float kx = _checked ? (Width - d - 3f) : 3f;
            using (SolidBrush kb = new SolidBrush(Color.White))
                g.FillEllipse(kb, kx, 3f, d, d);
        }
    }

    public class ModernColorDot : Control
    {
        public Color SwatchColor { get; set; }
        public bool Selected { get; set; }
        private bool _hovered;

        public ModernColorDot()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Size = new Size(32, 32);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Selected || _hovered)
            {
                using (Pen ring = new Pen(Selected ? Color.White : Color.FromArgb(140, 160, 200), Selected ? 2f : 1.3f))
                    g.DrawEllipse(ring, 2f, 2f, Width - 4f, Height - 4f);
            }
            using (SolidBrush b = new SolidBrush(SwatchColor))
                g.FillEllipse(b, 5.5f, 5.5f, Width - 11f, Height - 11f);
        }
    }
}
