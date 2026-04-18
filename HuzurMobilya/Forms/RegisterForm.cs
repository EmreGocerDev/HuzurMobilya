using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HuzurMobilya.Services;

namespace HuzurMobilya.Forms
{
    public class RegisterForm : Form
    {
        private TextBox txtName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private TextBox txtPassword = null!;
        private TextBox txtPasswordConfirm = null!;
        private Label lblError = null!;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Huzur Mobilya - Kayıt Ol";
            Size = new Size(520, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;

            // Gradient header
            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 90 };
            headerPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new LinearGradientBrush(headerPanel.ClientRectangle,
                    Theme.GradientStart, Theme.GradientEnd, 135f);
                g.FillRectangle(brush, headerPanel.ClientRectangle);

                using var circleBrush = new SolidBrush(Color.FromArgb(15, 255, 255, 255));
                g.FillEllipse(circleBrush, 350, -30, 120, 120);
            };

            var lblHeader = new Label
            {
                Text = "🏠  Yeni Hesap Oluşturun", Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true,
                Location = new Point(28, 28), BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblHeader);

            int y = 110;
            int left = 55;
            int fieldW = 400;

            Label FieldLabel(string text, int yy) => new Label
            {
                Text = text.ToUpper(), Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(left, yy)
            };

            Controls.Add(FieldLabel("Ad Soyad", y));
            txtName = Theme.CreateTextBox("Ad Soyad", fieldW);
            txtName.Location = new Point(left, y + 18); Controls.Add(txtName);

            y += 66;
            Controls.Add(FieldLabel("E-Posta", y));
            txtEmail = Theme.CreateTextBox("ornek@huzurmobilya.com", fieldW);
            txtEmail.Location = new Point(left, y + 18); Controls.Add(txtEmail);

            y += 66;
            Controls.Add(FieldLabel("Telefon", y));
            txtPhone = Theme.CreateTextBox("0555 000 0000", fieldW);
            txtPhone.Location = new Point(left, y + 18); Controls.Add(txtPhone);

            y += 66;
            Controls.Add(FieldLabel("Şifre", y));
            txtPassword = Theme.CreateTextBox("En az 6 karakter", fieldW);
            txtPassword.Location = new Point(left, y + 18);
            txtPassword.UseSystemPasswordChar = true; Controls.Add(txtPassword);

            y += 66;
            Controls.Add(FieldLabel("Şifre Tekrar", y));
            txtPasswordConfirm = Theme.CreateTextBox("Şifrenizi tekrar giriniz", fieldW);
            txtPasswordConfirm.Location = new Point(left, y + 18);
            txtPasswordConfirm.UseSystemPasswordChar = true; Controls.Add(txtPasswordConfirm);

            y += 70;
            var btnRegister = Theme.CreateButton("  KAYIT OL  →", Theme.Primary, fieldW, 48);
            btnRegister.Location = new Point(left, y);
            btnRegister.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnRegister.Click += BtnRegister_Click;
            Controls.Add(btnRegister);

            lblError = new Label
            {
                Text = "", ForeColor = Theme.Danger, Font = Theme.SmallFont,
                Location = new Point(left, y + 55), MaximumSize = new Size(fieldW, 0), AutoSize = true
            };

            Controls.AddRange(new Control[] { headerPanel, lblError });
            AcceptButton = btnRegister;
        }

        private async void BtnRegister_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text)
                || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Lütfen tüm zorunlu alanları doldurunuz.";
                return;
            }
            if (txtPassword.Text.Length < 6)
            {
                lblError.Text = "Şifre en az 6 karakter olmalıdır.";
                return;
            }
            if (txtPassword.Text != txtPasswordConfirm.Text)
            {
                lblError.Text = "Şifreler eşleşmiyor.";
                return;
            }

            try
            {
                await SupabaseService.RegisterAsync(
                    txtName.Text.Trim(), txtEmail.Text.Trim(),
                    txtPassword.Text, txtPhone.Text.Trim());

                MessageBox.Show("Kayıt başarılı! Giriş yapabilirsiniz.", "Başarılı ✅",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "Kayıt hatası: " + ex.Message;
            }
        }
    }
}
