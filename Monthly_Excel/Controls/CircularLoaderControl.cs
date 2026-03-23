using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Monthly_Excel.UI;

namespace Monthly_Excel.Controls
{
    internal sealed class CircularLoaderControl : Control
    {
        private readonly System.Windows.Forms.Timer _animationTimer;
        private int _startAngle;

        public CircularLoaderControl()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(36, 36);
            ForeColor = AppTheme.Accent;
            BackColor = Color.Transparent;

            _animationTimer = new System.Windows.Forms.Timer { Interval = 80 };
            _animationTimer.Tick += (_, _) =>
            {
                _startAngle = (_startAngle + 24) % 360;
                Invalidate();
            };
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                _animationTimer.Start();
            }
            else
            {
                _animationTimer.Stop();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(4, 4, Width - 8, Height - 8);

            using var trackPen = new Pen(Color.FromArgb(218, 226, 233), 4f);
            using var activePen = new Pen(ForeColor, 4.5f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            e.Graphics.DrawArc(trackPen, bounds, 0, 360);
            e.Graphics.DrawArc(activePen, bounds, _startAngle, 105);
        }
    }
}
