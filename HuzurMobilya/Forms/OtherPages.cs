using System;
using System.Drawing;
using System.Windows.Forms;
using HuzurMobilya.Models;
using HuzurMobilya.Services;

namespace HuzurMobilya.Forms
{
    // ── Kategoriler Sayfası ──
    public class CategoriesPage : UserControl
    {
        private DataGridView dgv = null!;

        public CategoriesPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56, FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4, 8, 4, 8), BackColor = Theme.Surface
            };

            var btnAdd = Theme.CreateButton("+ Yeni Kategori", Theme.Primary, 160);
            btnAdd.Click += (s, e) =>
            {
                var f = new CategoryEditForm(null);
                if (f.ShowDialog() == DialogResult.OK) LoadData();
            };

            var btnRefresh = Theme.CreateButton("🔄 Yenile", Theme.Info, 100);
            btnRefresh.Click += (s, e) => LoadData();

            toolbar.Controls.AddRange(new Control[] { btnAdd, btnRefresh });
            Controls.Add(toolbar);

            dgv = Theme.CreateDataGrid();
            dgv.Dock = DockStyle.Fill;
            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var cat = dgv.Rows[e.RowIndex].Tag as Category;
                if (cat != null) { var f = new CategoryEditForm(cat); if (f.ShowDialog() == DialogResult.OK) LoadData(); }
            };
            Controls.Add(dgv);
            dgv.BringToFront(); toolbar.BringToFront();

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var cats = await SupabaseService.GetCategoriesAsync();
                dgv.Columns.Clear(); dgv.Rows.Clear();
                dgv.Columns.Add("Name", "Kategori Adı");
                dgv.Columns.Add("Desc", "Açıklama");
                dgv.Columns.Add("Order", "Sıra");
                dgv.Columns.Add("Active", "Aktif");

                foreach (var c in cats)
                {
                    dgv.Rows.Add(c.Name, c.Description ?? "-", c.SortOrder, c.IsActive ? "✅" : "❌");
                    dgv.Rows[^1].Tag = c;
                }
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }

    public class CategoryEditForm : Form
    {
        private Category? _cat;
        private TextBox txtName = null!, txtDesc = null!, txtOrder = null!;
        private CheckBox chkActive = null!;

        public CategoryEditForm(Category? cat)
        {
            _cat = cat;
            Text = cat == null ? "Yeni Kategori" : "Kategori Düzenle";
            Size = new Size(420, 360);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 10, 30, 10) };
            Controls.Add(panel);

            var header = Theme.CreateModernHeader("📂", cat == null ? "Yeni Kategori" : "Kategori Düzenle");
            Controls.Add(header);
            int y = 10;
            int leftMargin = 15;

            void Add(string label, Control ctrl)
            {
                panel.Controls.Add(new Label { Text = label, Font = Theme.BodyFont, Location = new Point(leftMargin, y), AutoSize = true });
                ctrl.Location = new Point(leftMargin, y + 22); ctrl.Width = 340; panel.Controls.Add(ctrl); y += 55;
            }

            txtName = Theme.CreateTextBox("Kategori adı"); Add("Kategori Adı *", txtName);
            txtDesc = Theme.CreateTextBox("Açıklama"); Add("Açıklama", txtDesc);
            txtOrder = Theme.CreateTextBox("0"); Add("Sıra Numarası", txtOrder);

            chkActive = new CheckBox { Text = "Aktif", Checked = true, Font = Theme.BodyFont, Location = new Point(leftMargin, y) };
            panel.Controls.Add(chkActive); y += 35;

            var btnSave = Theme.CreateButton("💾  Kaydet", Theme.Success, 160, 44);
            btnSave.Location = new Point(leftMargin, y); btnSave.Click += BtnSave_Click;
            var btnCancel = Theme.CreateButton("İptal", Color.Gray, 160, 44);
            btnCancel.Location = new Point(leftMargin + 180, y); btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            panel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            if (cat != null)
            {
                txtName.Text = cat.Name; txtDesc.Text = cat.Description ?? "";
                txtOrder.Text = cat.SortOrder.ToString(); chkActive.Checked = cat.IsActive;
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Kategori adı zorunludur."); return; }
            var c = _cat ?? new Category();
            c.Name = txtName.Text.Trim(); c.Description = txtDesc.Text;
            c.SortOrder = int.TryParse(txtOrder.Text, out var o) ? o : 0;
            c.IsActive = chkActive.Checked;

            try { await SupabaseService.SaveCategoryAsync(c); DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }

    // ── Tedarikçiler Sayfası ──
    public class SuppliersPage : UserControl
    {
        private DataGridView dgv = null!;

        public SuppliersPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56, FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4, 8, 4, 8), BackColor = Theme.Surface
            };

            var btnAdd = Theme.CreateButton("+ Yeni Tedarikçi", Theme.Primary, 170);
            btnAdd.Click += (s, e) =>
            {
                var f = new SupplierEditForm(null);
                if (f.ShowDialog() == DialogResult.OK) LoadData();
            };

            var btnRefresh = Theme.CreateButton("🔄 Yenile", Theme.Info, 100);
            btnRefresh.Click += (s, e) => LoadData();

            toolbar.Controls.AddRange(new Control[] { btnAdd, btnRefresh });
            Controls.Add(toolbar);

            dgv = Theme.CreateDataGrid();
            dgv.Dock = DockStyle.Fill;
            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var sup = dgv.Rows[e.RowIndex].Tag as Supplier;
                if (sup != null) { var f = new SupplierEditForm(sup); if (f.ShowDialog() == DialogResult.OK) LoadData(); }
            };
            Controls.Add(dgv);
            dgv.BringToFront(); toolbar.BringToFront();

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var sups = await SupabaseService.GetSuppliersAsync();
                dgv.Columns.Clear(); dgv.Rows.Clear();
                dgv.Columns.Add("Company", "Firma Adı");
                dgv.Columns.Add("Contact", "İlgili Kişi");
                dgv.Columns.Add("Phone", "Telefon");
                dgv.Columns.Add("Email", "E-posta");
                dgv.Columns.Add("City", "Şehir");
                dgv.Columns.Add("Tax", "Vergi No");
                dgv.Columns.Add("Active", "Aktif");

                foreach (var s in sups)
                {
                    dgv.Rows.Add(s.CompanyName, s.ContactName ?? "-", s.Phone ?? "-",
                        s.Email ?? "-", s.City ?? "-", s.TaxNumber ?? "-",
                        s.IsActive ? "✅" : "❌");
                    dgv.Rows[^1].Tag = s;
                }
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }

    public class SupplierEditForm : Form
    {
        private Supplier? _sup;
        private TextBox txtCompany = null!, txtContact = null!, txtEmail = null!;
        private TextBox txtPhone = null!, txtAddress = null!, txtCity = null!;
        private TextBox txtTaxNo = null!, txtTaxOffice = null!, txtNotes = null!;
        private CheckBox chkActive = null!;

        public SupplierEditForm(Supplier? sup)
        {
            _sup = sup;
            Text = sup == null ? "Yeni Tedarikçi" : "Tedarikçi Düzenle";
            Size = new Size(480, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;

            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(30, 10, 30, 10) };
            Controls.Add(panel);

            var header = Theme.CreateModernHeader("🏭", sup == null ? "Yeni Tedarikçi" : "Tedarikçi Düzenle");
            Controls.Add(header);
            int y = 10;
            int leftMargin = 15;

            void Add(string label, Control ctrl)
            {
                panel.Controls.Add(new Label { Text = label, Font = Theme.BodyFont, Location = new Point(leftMargin, y), AutoSize = true });
                ctrl.Location = new Point(leftMargin, y + 22); ctrl.Width = 400; panel.Controls.Add(ctrl); y += 55;
            }

            txtCompany = Theme.CreateTextBox("Firma adı"); Add("Firma Adı *", txtCompany);
            txtContact = Theme.CreateTextBox("İlgili kişi"); Add("İlgili Kişi", txtContact);
            txtPhone = Theme.CreateTextBox("Telefon"); Add("Telefon", txtPhone);
            txtEmail = Theme.CreateTextBox("E-posta"); Add("E-posta", txtEmail);
            txtAddress = Theme.CreateTextBox("Adres"); Add("Adres", txtAddress);
            txtCity = Theme.CreateTextBox("Şehir"); Add("Şehir", txtCity);
            txtTaxNo = Theme.CreateTextBox("Vergi No"); Add("Vergi No", txtTaxNo);
            txtTaxOffice = Theme.CreateTextBox("Vergi Dairesi"); Add("Vergi Dairesi", txtTaxOffice);
            txtNotes = Theme.CreateTextBox("Notlar"); Add("Notlar", txtNotes);

            chkActive = new CheckBox { Text = "Aktif", Checked = true, Font = Theme.BodyFont, Location = new Point(leftMargin, y) };
            panel.Controls.Add(chkActive); y += 35;

            var btnSave = Theme.CreateButton("💾  Kaydet", Theme.Success, 190, 44);
            btnSave.Location = new Point(leftMargin, y); btnSave.Click += BtnSave_Click;
            var btnCancel = Theme.CreateButton("İptal", Color.Gray, 190, 44);
            btnCancel.Location = new Point(leftMargin + 210, y); btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            panel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            if (sup != null)
            {
                txtCompany.Text = sup.CompanyName; txtContact.Text = sup.ContactName ?? "";
                txtPhone.Text = sup.Phone ?? ""; txtEmail.Text = sup.Email ?? "";
                txtAddress.Text = sup.Address ?? ""; txtCity.Text = sup.City ?? "";
                txtTaxNo.Text = sup.TaxNumber ?? ""; txtTaxOffice.Text = sup.TaxOffice ?? "";
                txtNotes.Text = sup.Notes ?? ""; chkActive.Checked = sup.IsActive;
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCompany.Text)) { MessageBox.Show("Firma adı zorunludur."); return; }
            var s = _sup ?? new Supplier();
            s.CompanyName = txtCompany.Text.Trim(); s.ContactName = txtContact.Text;
            s.Phone = txtPhone.Text; s.Email = txtEmail.Text; s.Address = txtAddress.Text;
            s.City = txtCity.Text; s.TaxNumber = txtTaxNo.Text; s.TaxOffice = txtTaxOffice.Text;
            s.Notes = txtNotes.Text; s.IsActive = chkActive.Checked;

            try { await SupabaseService.SaveSupplierAsync(s); DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }

    // ── Bildirimler Sayfası ──
    public class NotificationsPage : UserControl
    {
        private DataGridView dgv = null!;

        public NotificationsPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56, FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4, 8, 4, 8), BackColor = Theme.Surface
            };

            var btnRefresh = Theme.CreateButton("🔄 Yenile", Theme.Info, 100);
            btnRefresh.Click += (s, e) => LoadData();

            toolbar.Controls.Add(btnRefresh);
            Controls.Add(toolbar);

            dgv = Theme.CreateDataGrid();
            dgv.Dock = DockStyle.Fill;
            dgv.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var notif = dgv.Rows[e.RowIndex].Tag as Notification;
                if (notif != null && !notif.IsRead)
                {
                    try
                    {
                        await SupabaseService.MarkNotificationReadAsync(notif.Id);
                        LoadData();
                    }
                    catch { }
                }
            };
            Controls.Add(dgv);
            dgv.BringToFront(); toolbar.BringToFront();

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var userId = SupabaseService.CurrentUser?.Id;
                if (string.IsNullOrEmpty(userId)) return;

                var notifs = await SupabaseService.GetNotificationsAsync(userId);
                dgv.Columns.Clear(); dgv.Rows.Clear();
                dgv.Columns.Add("Read", "");
                dgv.Columns[0].Width = 30;
                dgv.Columns.Add("Title", "Başlık");
                dgv.Columns.Add("Message", "Mesaj");
                dgv.Columns.Add("Date", "Tarih");

                foreach (var n in notifs)
                {
                    dgv.Rows.Add(n.IsRead ? "📭" : "📬", n.Title, n.Message,
                        n.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                    dgv.Rows[^1].Tag = n;
                    if (!n.IsRead)
                        dgv.Rows[^1].DefaultCellStyle.BackColor = Theme.PrimaryLight;
                }
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }
}
