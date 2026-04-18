using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using HuzurMobilya.Services;

namespace HuzurMobilya.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Button btnRegister = null!;
        private Label lblError = null!;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Huzur Mobilya - Giriş";
            Size = new Size(1000, 620);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Theme.Background;

            // Custom title bar
            var titleBar = Theme.CreateCustomTitleBar(this, "Huzur Mobilya - Giriş", false);
            Controls.Add(titleBar);

            // Left panel (gradient brand area)
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left, Width = 440
            };
            leftPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new LinearGradientBrush(leftPanel.ClientRectangle,
                    Color.FromArgb(79, 70, 229), Color.FromArgb(139, 92, 246), 135f);
                g.FillRectangle(brush, leftPanel.ClientRectangle);

                // Decorative circles
                using var circleBrush = new SolidBrush(Color.FromArgb(20, 255, 255, 255));
                g.FillEllipse(circleBrush, -60, -60, 250, 250);
                g.FillEllipse(circleBrush, 280, 350, 200, 200);
                using var circleBrush2 = new SolidBrush(Color.FromArgb(10, 255, 255, 255));
                g.FillEllipse(circleBrush2, 150, 100, 350, 350);
            };

            // Logo image centered
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo", "logo.png");
            if (File.Exists(logoPath))
            {
                var picLogo = new PictureBox
                {
                    Image = Image.FromFile(logoPath),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(280, 200),
                    Location = new Point(80, 120),
                    BackColor = Color.Transparent
                };
                leftPanel.Controls.Add(picLogo);
            }

            var lblSlogan = new Label
            {
                Text = "Stok & Personel Yönetim Sistemi",
                Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(200, 220, 255),
                AutoSize = true, Location = new Point(88, 340), BackColor = Color.Transparent
            };

            var lblVersion = new Label
            {
                Text = "v1.0  ·  Modern Yönetim Paneli",
                Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(150, 180, 240),
                AutoSize = true, Location = new Point(115, 500), BackColor = Color.Transparent
            };

            leftPanel.Controls.AddRange(new Control[] { lblSlogan, lblVersion });

            // Right panel (login form area)
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.White,
                Padding = new Padding(60, 0, 60, 0)
            };

            var lblWelcome = new Label
            {
                Text = "👋 Hoş Geldiniz!", Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Theme.TextPrimary, AutoSize = true, Location = new Point(60, 80)
            };

            var lblSubtitle = new Label
            {
                Text = "Devam etmek için hesabınıza giriş yapın",
                Font = new Font("Segoe UI", 10), ForeColor = Theme.TextSecondary,
                AutoSize = true, Location = new Point(60, 125)
            };

            var lblEmail = new Label
            {
                Text = "E-POSTA ADRESİ", Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(60, 190)
            };

            txtEmail = Theme.CreateTextBox("ornek@huzurmobilya.com", 420);
            txtEmail.Location = new Point(60, 212);
            txtEmail.Font = new Font("Segoe UI", 11);
            txtEmail.Height = 40;

            var lblPass = new Label
            {
                Text = "ŞİFRE", Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(60, 270)
            };

            txtPassword = Theme.CreateTextBox("••••••••", 420);
            txtPassword.Location = new Point(60, 292);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Font = new Font("Segoe UI", 11);
            txtPassword.Height = 40;

            btnLogin = Theme.CreateButton("  GİRİŞ YAP  →", Theme.Primary, 420, 48);
            btnLogin.Location = new Point(60, 360);
            btnLogin.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnLogin.Click += BtnLogin_Click;

            var lblOr = new Label
            {
                Text = "─────────────  veya  ─────────────",
                Font = new Font("Segoe UI", 8), ForeColor = Theme.TextSecondary,
                AutoSize = true, Location = new Point(92, 422)
            };

            btnRegister = new Button
            {
                Text = "  Hesabınız yok mu? Kayıt Olun  ",
                Size = new Size(420, 48), Location = new Point(60, 452),
                FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                ForeColor = Theme.Primary, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderColor = Theme.Primary;
            btnRegister.FlatAppearance.BorderSize = 2;
            btnRegister.FlatAppearance.MouseOverBackColor = Theme.PrimaryLight;
            btnRegister.Click += BtnRegister_Click;

            lblError = new Label
            {
                Text = "", ForeColor = Theme.Danger, Font = Theme.SmallFont,
                Location = new Point(60, 510), AutoSize = true, MaximumSize = new Size(420, 0)
            };

            rightPanel.Controls.AddRange(new Control[] { lblWelcome, lblSubtitle, lblEmail,
                txtEmail, lblPass, txtPassword, btnLogin, lblOr, btnRegister, lblError });

            Controls.AddRange(new Control[] { rightPanel, leftPanel });
            AcceptButton = btnLogin;
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Lütfen e-posta ve şifre giriniz.";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "  Giriş yapılıyor...";
            try
            {
                var user = await SupabaseService.LoginAsync(txtEmail.Text.Trim(), txtPassword.Text);
                if (user == null)
                {
                    lblError.Text = "E-posta veya şifre hatalı.";
                    return;
                }
                var main = new MainForm();
                main.Show();
                Hide();
            }
            catch (Exception ex)
            {
                lblError.Text = "Bağlantı hatası: " + ex.Message;
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "  GİRİŞ YAP  →";
            }
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            var reg = new RegisterForm();
            reg.ShowDialog();
        }
    }
}
