using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using HuzurMobilya.Services;

namespace HuzurMobilya.Forms
{
    public class MainForm : Form
    {
        private Panel sidebar = null!;
        private Panel contentPanel = null!;
        private Panel headerPanel = null!;
        private Label lblPageTitle = null!;
        private Label lblUserName = null!;
        private Button? activeButton;

        public MainForm()
        {
            InitializeComponent();
            LoadPage(new DashboardPage());
            SetActiveButton(sidebar.Controls[1] as Button);
        }

        private void InitializeComponent()
        {
            Text = "Huzur Mobilya - Yönetim Paneli";
            Size = new Size(1440, 880);
            MinimumSize = new Size(1200, 700);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Theme.Background;

            // ── Modern Sidebar ──
            sidebar = new Panel
            {
                Dock = DockStyle.Left, Width = 260,
                BackColor = Theme.SidebarBg
            };

            // Logo area with gradient
            var logoPanel = new Panel { Height = 80, Dock = DockStyle.Top };
            logoPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new LinearGradientBrush(
                    logoPanel.ClientRectangle,
                    Color.FromArgb(79, 70, 229), Color.FromArgb(139, 92, 246), 135f);
                g.FillRectangle(brush, logoPanel.ClientRectangle);
            };
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo", "logo.png");
            if (File.Exists(logoPath))
            {
                var picLogo = new PictureBox
                {
                    Image = Image.FromFile(logoPath),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(200, 60),
                    Location = new Point(30, 10),
                    BackColor = Color.Transparent
                };
                logoPanel.Controls.Add(picLogo);
            }
            sidebar.Controls.Add(logoPanel);

            // Menu items
            var menuLabel = new Label
            {
                Text = "   MENÜ", Font = new Font("Segoe UI", 7, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99), Dock = DockStyle.Top,
                Height = 32, TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(20, 0, 0, 4)
            };
            sidebar.Controls.Add(menuLabel);
            sidebar.Controls.SetChildIndex(menuLabel, 1);

            AddSidebarButton("📊  Dashboard", (s, e) => { LoadPage(new DashboardPage()); SetActiveButton(s as Button); });
            AddSidebarButton("📦  Ürünler", (s, e) => { LoadPage(new ProductsPage()); SetActiveButton(s as Button); });
            AddSidebarButton("📋  Stok Takibi", (s, e) => { LoadPage(new StockPage()); SetActiveButton(s as Button); });
            AddSidebarButton("🔄  Stok Hareketleri", (s, e) => { LoadPage(new StockMovementsPage()); SetActiveButton(s as Button); });
            AddSidebarButton("👥  Müşteriler", (s, e) => { LoadPage(new CustomersPage()); SetActiveButton(s as Button); });
            AddSidebarButton("🛒  Siparişler", (s, e) => { LoadPage(new OrdersPage()); SetActiveButton(s as Button); });
            AddSidebarButton("👨‍💼  Personel", (s, e) => { LoadPage(new EmployeesPage()); SetActiveButton(s as Button); });
            AddSidebarButton("📂  Kategoriler", (s, e) => { LoadPage(new CategoriesPage()); SetActiveButton(s as Button); });
            AddSidebarButton("🏭  Tedarikçiler", (s, e) => { LoadPage(new SuppliersPage()); SetActiveButton(s as Button); });
            AddSidebarButton("🔔  Bildirimler", (s, e) => { LoadPage(new NotificationsPage()); SetActiveButton(s as Button); });

            // Logout at bottom
            var btnLogout = new Button
            {
                Text = "🚪  Çıkış Yap", Dock = DockStyle.Bottom, Height = 52,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 239, 68, 68),
                ForeColor = Color.FromArgb(252, 165, 165),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(127, 29, 29);
            btnLogout.Click += (s, e) =>
            {
                SupabaseService.CurrentUser = null;
                var login = new LoginForm();
                login.Show();
                Close();
            };
            sidebar.Controls.Add(btnLogout);

            // ── Modern Header ──
            headerPanel = new Panel
            {
                Dock = DockStyle.Top, Height = 64,
                BackColor = Theme.Surface, Padding = new Padding(24, 0, 24, 0)
            };
            headerPanel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(243, 244, 246), 1);
                e.Graphics.DrawLine(pen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
            };

            lblPageTitle = new Label
            {
                Text = "Dashboard", Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Theme.TextPrimary, AutoSize = true,
                Location = new Point(24, 18)
            };

            var user = SupabaseService.CurrentUser;
            var userBadge = new Panel
            {
                Size = new Size(280, 36),
                BackColor = Theme.PrimaryLight, Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblUserName = new Label
            {
                Text = $"👤  {user?.FullName ?? "Kullanıcı"}  •  {user?.Role ?? ""}",
                Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Theme.Primary,
                AutoSize = false, Size = new Size(280, 36),
                TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent
            };
            userBadge.Controls.Add(lblUserName);
            userBadge.Location = new Point(headerPanel.Width - 310, 14);
            headerPanel.Resize += (s, e) =>
            {
                userBadge.Location = new Point(headerPanel.Width - 310, 14);
            };

            headerPanel.Controls.AddRange(new Control[] { lblPageTitle, userBadge });

            // ── Content ──
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Theme.Background,
                Padding = new Padding(24)
            };

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
            Controls.Add(sidebar);

            // Custom title bar
            var titleBar = Theme.CreateCustomTitleBar(this, "Huzur Mobilya - Yönetim Paneli");
            Controls.Add(titleBar);
            titleBar.BringToFront();
        }

        private void AddSidebarButton(string text, EventHandler click)
        {
            var btn = new Button
            {
                Text = text, Dock = DockStyle.Top, Height = 46,
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.SidebarBg,
                ForeColor = Theme.SidebarText,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Theme.SidebarHover;
            btn.Click += click;

            sidebar.Controls.Add(btn);
            sidebar.Controls.SetChildIndex(btn, 2);
        }

        private void SetActiveButton(Button? btn)
        {
            if (activeButton != null)
            {
                activeButton.BackColor = Theme.SidebarBg;
                activeButton.ForeColor = Theme.SidebarText;
                activeButton.Font = new Font("Segoe UI", 10);
            }
            if (btn != null)
            {
                btn.BackColor = Color.FromArgb(30, 99, 102, 241);
                btn.ForeColor = Color.FromArgb(165, 180, 252);
                btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                activeButton = btn;
                lblPageTitle.Text = btn.Text.Replace("📊", "").Replace("📦", "")
                    .Replace("📋", "").Replace("🔄", "").Replace("👥", "")
                    .Replace("🛒", "").Replace("👨‍💼", "").Replace("📂", "")
                    .Replace("🏭", "").Replace("🔔", "").Replace("🚪", "").Trim();
            }
        }

        public void LoadPage(UserControl page)
        {
            contentPanel.Controls.Clear();
            page.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(page);
        }
    }
}
