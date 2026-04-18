using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HuzurMobilya
{
    public static class Theme
    {
        // ── Modern Color Palette (Indigo/Violet tones) ──
        public static readonly Color Primary = Color.FromArgb(99, 102, 241);        // Indigo-500
        public static readonly Color PrimaryDark = Color.FromArgb(79, 70, 229);      // Indigo-600
        public static readonly Color PrimaryLight = Color.FromArgb(238, 242, 255);   // Indigo-50
        public static readonly Color Accent = Color.FromArgb(139, 92, 246);          // Violet-500
        public static readonly Color Success = Color.FromArgb(16, 185, 129);         // Emerald-500
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);         // Amber-500
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);           // Red-500
        public static readonly Color Info = Color.FromArgb(6, 182, 212);             // Cyan-500
        public static readonly Color Background = Color.FromArgb(245, 247, 251);     // Cool Gray bg
        public static readonly Color Surface = Color.White;
        public static readonly Color SidebarBg = Color.FromArgb(17, 24, 39);         // Gray-900
        public static readonly Color SidebarText = Color.FromArgb(156, 163, 175);    // Gray-400
        public static readonly Color SidebarHover = Color.FromArgb(31, 41, 55);      // Gray-800
        public static readonly Color SidebarActive = Color.FromArgb(99, 102, 241);   // Indigo-500
        public static readonly Color TextPrimary = Color.FromArgb(17, 24, 39);       // Gray-900
        public static readonly Color TextSecondary = Color.FromArgb(107, 114, 128);  // Gray-500
        public static readonly Color Border = Color.FromArgb(229, 231, 235);         // Gray-200
        public static readonly Color CardShadow = Color.FromArgb(30, 0, 0, 0);       // Subtle shadow
        public static readonly Color GradientStart = Color.FromArgb(99, 102, 241);   // Indigo
        public static readonly Color GradientEnd = Color.FromArgb(139, 92, 246);     // Violet

        public static readonly Font TitleFont = new("Segoe UI", 22, FontStyle.Bold);
        public static readonly Font SubtitleFont = new("Segoe UI", 14, FontStyle.Bold);
        public static readonly Font BodyFont = new("Segoe UI", 10);
        public static readonly Font SmallFont = new("Segoe UI", 9);
        public static readonly Font ButtonFont = new("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font CardValueFont = new("Segoe UI", 26, FontStyle.Bold);
        public static readonly Font CardTitleFont = new("Segoe UI", 9);

        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int l, int t, int r, int b, int w, int h);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static Button CreateButton(string text, Color bgColor, int width = 140, int height = 40)
        {
            var btn = new Button
            {
                Text = text, Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat, BackColor = bgColor,
                ForeColor = Color.White, Font = ButtonFont,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(bgColor, 0.05f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bgColor, 0.15f);
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, width, height, 12, 12));
            return btn;
        }

        public static TextBox CreateTextBox(string placeholder = "", int width = 300)
        {
            var txt = new TextBox
            {
                Size = new Size(width, 38), Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = placeholder,
                BackColor = Color.FromArgb(249, 250, 251)
            };
            return txt;
        }

        public static Label CreateLabel(string text, Font? font = null, Color? color = null)
        {
            return new Label
            {
                Text = text, AutoSize = true,
                Font = font ?? BodyFont,
                ForeColor = color ?? TextPrimary
            };
        }

        public static Panel CreateCard(int width, int height)
        {
            var panel = new Panel
            {
                Size = new Size(width, height), BackColor = Surface,
                Padding = new Padding(24), Margin = new Padding(0, 0, 0, 12)
            };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                using var path = RoundedRect(rect, 12);
                using var shadowBrush = new SolidBrush(Color.FromArgb(15, 0, 0, 0));
                g.FillPath(shadowBrush, RoundedRect(new Rectangle(2, 3, panel.Width - 1, panel.Height - 1), 12));
                using var fillBrush = new SolidBrush(Surface);
                g.FillPath(fillBrush, path);
                using var borderPen = new Pen(Border, 1);
                g.DrawPath(borderPen, path);
            };
            return panel;
        }

        public static DataGridView CreateDataGrid()
        {
            var dgv = new DataGridView
            {
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Surface, BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, Font = BodyFont,
                GridColor = Color.FromArgb(243, 244, 246), AllowUserToResizeRows = false
            };
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251), ForeColor = TextSecondary,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(12, 8, 12, 8),
                SelectionBackColor = Color.FromArgb(249, 250, 251),
                SelectionForeColor = TextSecondary
            };
            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Surface, ForeColor = TextPrimary,
                SelectionBackColor = PrimaryLight, SelectionForeColor = TextPrimary,
                Padding = new Padding(12, 6, 12, 6)
            };
            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(249, 250, 251)
            };
            dgv.ColumnHeadersHeight = 48;
            dgv.RowTemplate.Height = 44;
            return dgv;
        }

        public static Panel CreateStatCard(string title, string value, string icon, Color accentColor, int width = 220)
        {
            var card = new Panel
            {
                Size = new Size(width, 120), BackColor = Color.Transparent,
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 16, 0)
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Shadow
                using var shadowPath = RoundedRect(new Rectangle(2, 3, card.Width - 4, card.Height - 4), 16);
                using var shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0));
                g.FillPath(shadowBrush, shadowPath);

                // Card body
                var rect = new Rectangle(0, 0, card.Width - 3, card.Height - 4);
                using var path = RoundedRect(rect, 16);
                using var fill = new SolidBrush(Surface);
                g.FillPath(fill, path);

                // Top accent gradient bar
                using var accentPath = new GraphicsPath();
                accentPath.AddArc(rect.X, rect.Y, 32, 32, 180, 90);
                accentPath.AddArc(rect.Right - 32, rect.Y, 32, 32, 270, 90);
                accentPath.AddLine(rect.Right, rect.Y + 16, rect.Right, rect.Y + 5);
                accentPath.AddLine(rect.X, rect.Y + 5, rect.X, rect.Y + 16);
                accentPath.CloseFigure();
                using var gradBrush = new LinearGradientBrush(
                    new Point(rect.X, 0), new Point(rect.Right, 0),
                    accentColor, ControlPaint.Light(accentColor, 0.3f));
                g.FillPath(gradBrush, accentPath);

                // Border
                using var borderPen = new Pen(Color.FromArgb(240, 240, 240), 1);
                g.DrawPath(borderPen, path);
            };

            var lblValue = new Label
            {
                Text = value, Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = TextPrimary, Location = new Point(20, 24),
                AutoSize = true, BackColor = Color.Transparent
            };

            var lblTitle = new Label
            {
                Text = title.ToUpper(), Font = CardTitleFont,
                ForeColor = TextSecondary, Location = new Point(20, 62),
                AutoSize = true, BackColor = Color.Transparent
            };

            var lblIcon = new Label
            {
                Text = icon, Font = new Font("Segoe UI", 22),
                ForeColor = Color.FromArgb(80, accentColor.R, accentColor.G, accentColor.B),
                Location = new Point(width - 52, 28),
                AutoSize = true, BackColor = Color.Transparent
            };

            card.Controls.AddRange(new Control[] { lblIcon, lblValue, lblTitle });
            return card;
        }

        public static Panel CreateModernHeader(string icon, string title)
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 70 };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new LinearGradientBrush(
                    header.ClientRectangle, GradientStart, GradientEnd, 135f);
                g.FillRectangle(brush, header.ClientRectangle);
            };
            var lbl = new Label
            {
                Text = $"{icon}  {title}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true,
                Location = new Point(24, 20), BackColor = Color.Transparent
            };
            header.Controls.Add(lbl);
            return header;
        }

        public static FlowLayoutPanel CreateModernToolbar()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4, 8, 4, 8),
                BackColor = Surface
            };
        }

        public static Panel CreateCustomTitleBar(Form form, string title, bool showMaximize = true)
        {
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.FromArgb(17, 24, 39) };

            void EnableDrag(Control ctrl)
            {
                ctrl.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); } };
            }
            EnableDrag(titleBar);

            var lblTitle = new Label
            {
                Text = title, ForeColor = Color.FromArgb(156, 163, 175), Font = new Font("Segoe UI", 8),
                AutoSize = true, Location = new Point(12, 9), BackColor = Color.Transparent
            };
            EnableDrag(lblTitle);
            titleBar.Controls.Add(lblTitle);

            var btnClose = new Button
            {
                Text = "\u2715", Size = new Size(46, 32), Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9), BackColor = Color.Transparent, Cursor = Cursors.Hand, TabStop = false
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
            btnClose.Click += (s, e) => form.Close();
            titleBar.Controls.Add(btnClose);

            if (showMaximize)
            {
                var btnMax = new Button
                {
                    Text = "\u2610", Size = new Size(46, 32), Dock = DockStyle.Right,
                    FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9), BackColor = Color.Transparent, Cursor = Cursors.Hand, TabStop = false
                };
                btnMax.FlatAppearance.BorderSize = 0;
                btnMax.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
                btnMax.Click += (s, e) =>
                {
                    form.WindowState = form.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                    btnMax.Text = form.WindowState == FormWindowState.Maximized ? "\u2750" : "\u2610";
                };
                titleBar.Controls.Add(btnMax);
            }

            var btnMin = new Button
            {
                Text = "\u2500", Size = new Size(46, 32), Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9), BackColor = Color.Transparent, Cursor = Cursors.Hand, TabStop = false
            };
            btnMin.FlatAppearance.BorderSize = 0;
            btnMin.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
            btnMin.Click += (s, e) => form.WindowState = FormWindowState.Minimized;
            titleBar.Controls.Add(btnMin);

            return titleBar;
        }
    }
}
