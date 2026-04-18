using System;
using System.Drawing;
using System.Windows.Forms;
using HuzurMobilya.Models;
using HuzurMobilya.Services;

namespace HuzurMobilya.Forms
{
    public class CustomersPage : UserControl
    {
        private DataGridView dgv = null!;
        private TextBox txtSearch = null!;
        private List<Customer> all = new();

        public CustomersPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56, FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4, 8, 4, 8), BackColor = Theme.Surface
            };
            txtSearch = Theme.CreateTextBox("🔍 Müşteri ara...", 300);
            txtSearch.TextChanged += (s, e) => Filter();

            var btnAdd = Theme.CreateButton("+ Yeni Müşteri", Theme.Primary, 160);
            btnAdd.Click += (s, e) => { var f = new CustomerEditForm(null); if (f.ShowDialog() == DialogResult.OK) LoadData(); };

            var btnRefresh = Theme.CreateButton("🔄 Yenile", Theme.Info, 100);
            btnRefresh.Click += (s, e) => LoadData();

            toolbar.Controls.AddRange(new Control[] { txtSearch, btnAdd, btnRefresh });
            Controls.Add(toolbar);

            dgv = Theme.CreateDataGrid();
            dgv.Dock = DockStyle.Fill;
            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var c = dgv.Rows[e.RowIndex].Tag as Customer;
                if (c != null) { var f = new CustomerEditForm(c); if (f.ShowDialog() == DialogResult.OK) LoadData(); }
            };
            Controls.Add(dgv);
            dgv.BringToFront(); toolbar.BringToFront();

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                all = await SupabaseService.GetCustomersAsync();
                BindGrid(all);
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void BindGrid(List<Customer> list)
        {
            dgv.Columns.Clear(); dgv.Rows.Clear();
            dgv.Columns.Add("Name", "Ad Soyad");
            dgv.Columns.Add("Phone", "Telefon");
            dgv.Columns.Add("Email", "E-posta");
            dgv.Columns.Add("City", "Şehir");
            dgv.Columns.Add("Orders", "Sipariş");
            dgv.Columns.Add("Spent", "Toplam Harcama");
            dgv.Columns.Add("Active", "Aktif");

            foreach (var c in list)
            {
                dgv.Rows.Add(c.FullName, c.Phone ?? "-", c.Email ?? "-", c.City ?? "-",
                    c.TotalOrders, $"₺{c.TotalSpent:N2}", c.IsActive ? "✅" : "❌");
                dgv.Rows[^1].Tag = c;
            }
        }

        private void Filter()
        {
            var q = txtSearch.Text.ToLower();
            BindGrid(all.FindAll(c => c.FullName.ToLower().Contains(q) ||
                (c.Phone?.Contains(q) ?? false) || (c.Email?.ToLower().Contains(q) ?? false)));
        }
    }

    public class CustomerEditForm : Form
    {
        private Customer? _cust;
        private TextBox txtName = null!, txtEmail = null!, txtPhone = null!;
        private TextBox txtAddress = null!, txtCity = null!, txtTax = null!, txtNotes = null!;
        private CheckBox chkActive = null!;

        public CustomerEditForm(Customer? cust)
        {
            _cust = cust;
            Text = cust == null ? "Yeni Müşteri" : "Müşteri Düzenle";
            Size = new Size(480, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;

            // Panel ÖNCE (arka Z), header SONRA (ön Z - Top'a yapışır)
            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Controls.Add(panel);

            var header = Theme.CreateModernHeader("👥", cust == null ? "Yeni Müşteri" : "Müşteri Düzenle");
            Controls.Add(header);

            int y = 14;
            int lm = 20;

            void Add(string label, Control ctrl)
            {
                panel.Controls.Add(new Label { Text = label, Font = Theme.BodyFont, Location = new Point(lm, y), AutoSize = true });
                ctrl.Location = new Point(lm, y + 22); ctrl.Width = 420; panel.Controls.Add(ctrl); y += 60;
            }

            txtName = Theme.CreateTextBox("Ad Soyad"); Add("Ad Soyad *", txtName);
            txtPhone = Theme.CreateTextBox("0555 000 0000"); Add("Telefon", txtPhone);
            txtEmail = Theme.CreateTextBox("ornek@mail.com"); Add("E-posta", txtEmail);
            txtAddress = Theme.CreateTextBox("Adres"); Add("Adres", txtAddress);
            txtCity = Theme.CreateTextBox("Şehir"); Add("Şehir", txtCity);
            txtTax = Theme.CreateTextBox("Vergi No"); Add("Vergi No", txtTax);
            txtNotes = Theme.CreateTextBox("Notlar"); Add("Notlar", txtNotes);

            chkActive = new CheckBox { Text = "Aktif", Checked = true, Font = Theme.BodyFont, Location = new Point(lm, y) };
            panel.Controls.Add(chkActive); y += 42;

            var btnSave = Theme.CreateButton("💾  Kaydet", Theme.Success, 200, 44);
            btnSave.Location = new Point(lm, y); btnSave.Click += BtnSave_Click;
            var btnCancel = Theme.CreateButton("İptal", Color.Gray, 200, 44);
            btnCancel.Location = new Point(lm + 216, y); btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            panel.Controls.AddRange(new Control[] { btnSave, btnCancel });
            y += 56;
            panel.AutoScrollMinSize = new Size(0, y);

            if (cust != null)
            {
                txtName.Text = cust.FullName; txtPhone.Text = cust.Phone ?? "";
                txtEmail.Text = cust.Email ?? ""; txtAddress.Text = cust.Address ?? "";
                txtCity.Text = cust.City ?? ""; txtTax.Text = cust.TaxNumber ?? "";
                txtNotes.Text = cust.Notes ?? ""; chkActive.Checked = cust.IsActive;
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Ad Soyad zorunludur."); return; }
            var c = _cust ?? new Customer();
            c.FullName = txtName.Text.Trim(); c.Phone = txtPhone.Text; c.Email = txtEmail.Text;
            c.Address = txtAddress.Text; c.City = txtCity.Text; c.TaxNumber = txtTax.Text;
            c.Notes = txtNotes.Text; c.IsActive = chkActive.Checked;

            try { await SupabaseService.SaveCustomerAsync(c); DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }
}
