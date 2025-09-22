using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
namespace CalendarApp
{
    public class ucDays : UserControl
    {
        private Label lblDay;
        private Label lblNote;
        public event EventHandler DayDoubleClicked;

        public ucDays()
        {
            this.Size = new Size(100, 80);
            this.Margin = new Padding(5);
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            lblDay = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30,
                Cursor = Cursors.Hand
            };
            lblNote = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Fill
            };
            this.Controls.Add(lblNote);
            this.Controls.Add(lblDay);
            this.DoubleClick += (s, e) => DayDoubleClicked?.Invoke(this, EventArgs.Empty);
            lblDay.DoubleClick += (s, e) => DayDoubleClicked?.Invoke(this, EventArgs.Empty);
            lblNote.DoubleClick += (s, e) => DayDoubleClicked?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int radius = 15;
            Rectangle rect = this.ClientRectangle;
            rect.Inflate(-1, -1);
            using (GraphicsPath path = RoundedRect(rect, radius))
            using (LinearGradientBrush brush = new LinearGradientBrush(rect,
                Color.FromArgb(255, 240, 248, 255),
                Color.White,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillPath(brush, path);
                using (Pen pen = new Pen(Color.LightGray, 2))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }
        private GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
        public void SetDay(string day) => lblDay.Text = day;
        public void SetNote(string note) => lblNote.Text = note;
    }
}
