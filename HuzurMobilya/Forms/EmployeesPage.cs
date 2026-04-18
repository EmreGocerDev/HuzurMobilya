using System;
using System.Drawing;
using System.Windows.Forms;
using HuzurMobilya.Models;
using HuzurMobilya.Services;

namespace HuzurMobilya.Forms
{
    public class EmployeesPage : UserControl
    {
        private DataGridView dgv = null!;
        private TextBox txtSearch = null!;
        private List<Employee> all = new();

        public EmployeesPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56, FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4, 8, 4, 8), BackColor = Theme.Surface
            };

            txtSearch = Theme.CreateTextBox("🔍 Personel ara...", 300);
            txtSearch.TextChanged += (s, e) => Filter();

            var btnAdd = Theme.CreateButton("+ Yeni Personel", Theme.Primary, 160);
            btnAdd.Click += (s, e) => { var f = new EmployeeEditForm(null); if (f.ShowDialog() == DialogResult.OK) LoadData(); };

            var btnLeave = Theme.CreateButton("📋 İzin Talepleri", Theme.Info, 160);
            btnLeave.Click += (s, e) =>
            {
                if (Parent?.Parent is MainForm mf) mf.LoadPage(new LeaveRequestsPage());
            };

            var btnRefresh = Theme.CreateButton("🔄 Yenile", Theme.Info, 100);
            btnRefresh.Click += (s, e) => LoadData();

            toolbar.Controls.AddRange(new Control[] { txtSearch, btnAdd, btnLeave, btnRefresh });
            Controls.Add(toolbar);

            dgv = Theme.CreateDataGrid();
            dgv.Dock = DockStyle.Fill;
            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var emp = dgv.Rows[e.RowIndex].Tag as Employee;
                if (emp != null) { var f = new EmployeeEditForm(emp); if (f.ShowDialog() == DialogResult.OK) LoadData(); }
            };
            Controls.Add(dgv);
            dgv.BringToFront(); toolbar.BringToFront();

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                all = await SupabaseService.GetEmployeesAsync();
                BindGrid(all);
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void BindGrid(List<Employee> list)
        {
            dgv.Columns.Clear(); dgv.Rows.Clear();
            dgv.Columns.Add("No", "Sicil No");
            dgv.Columns.Add("Name", "Ad Soyad");
            dgv.Columns.Add("Dept", "Departman");
            dgv.Columns.Add("Pos", "Pozisyon");
            dgv.Columns.Add("Phone", "Telefon");
            dgv.Columns.Add("Hire", "İşe Giriş");
            dgv.Columns.Add("Salary", "Maaş");
            dgv.Columns.Add("Status", "Durum");

            foreach (var emp in list)
            {
                var statusIcon = emp.Status switch
                {
                    "aktif" => "🟢",
                    "izinli" => "🟡",
                    "pasif" => "🔴",
                    "ayrildi" => "⚫",
                    _ => ""
                };
                dgv.Rows.Add(emp.EmployeeNo, emp.FullName ?? "-", emp.Department, emp.Position,
                    emp.Phone ?? "-", emp.HireDate.ToString("dd.MM.yyyy"),
                    emp.Salary.HasValue ? $"₺{emp.Salary:N2}" : "-",
                    $"{statusIcon} {emp.Status}");
                dgv.Rows[^1].Tag = emp;
            }
        }

        private void Filter()
        {
            var q = txtSearch.Text.ToLower();
            BindGrid(all.FindAll(e =>
                (e.FullName?.ToLower().Contains(q) ?? false) ||
                e.Department.ToLower().Contains(q) ||
                e.EmployeeNo.ToLower().Contains(q)));
        }
    }

    public class EmployeeEditForm : Form
    {
        private Employee? _emp;
        private TextBox txtName = null!, txtEmail = null!, txtPhone = null!, txtPassword = null!;
        private TextBox txtDept = null!, txtPos = null!, txtNationalId = null!;
        private TextBox txtAddress = null!, txtCity = null!, txtSalary = null!;
        private TextBox txtEmergencyContact = null!, txtEmergencyPhone = null!, txtNotes = null!;
        private DateTimePicker dtpHire = null!, dtpBirth = null!;
        private ComboBox cmbStatus = null!;

        public EmployeeEditForm(Employee? emp)
        {
            _emp = emp;
            Text = emp == null ? "Yeni Personel" : "Personel Düzenle";
            Size = new Size(550, 780);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;

            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(30, 10, 30, 10) };
            Controls.Add(panel);

            var header = Theme.CreateModernHeader("👨‍💼", emp == null ? "Yeni Personel" : "Personel Düzenle");
            Controls.Add(header);
            int y = 10;
            int leftMargin = 15;

            void Add(string label, Control ctrl)
            {
                panel.Controls.Add(new Label { Text = label, Font = Theme.BodyFont, Location = new Point(leftMargin, y), AutoSize = true });
                ctrl.Location = new Point(leftMargin, y + 22); ctrl.Width = 460; panel.Controls.Add(ctrl); y += 55;
            }

            txtName = Theme.CreateTextBox("Ad Soyad"); Add("Ad Soyad *", txtName);
            txtEmail = Theme.CreateTextBox("E-posta"); Add("E-posta *", txtEmail);
            txtPhone = Theme.CreateTextBox("Telefon"); Add("Telefon", txtPhone);

            if (emp == null)
            {
                txtPassword = Theme.CreateTextBox("Giriş şifresi");
                txtPassword.UseSystemPasswordChar = true;
                Add("Şifre *", txtPassword);
            }

            txtDept = Theme.CreateTextBox("Ör: Üretim"); Add("Departman *", txtDept);
            txtPos = Theme.CreateTextBox("Ör: Usta"); Add("Pozisyon *", txtPos);

            dtpHire = new DateTimePicker { Font = Theme.BodyFont, Format = DateTimePickerFormat.Short };
            Add("İşe Giriş Tarihi *", dtpHire);

            dtpBirth = new DateTimePicker { Font = Theme.BodyFont, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
            Add("Doğum Tarihi", dtpBirth);

            txtNationalId = Theme.CreateTextBox("TC Kimlik No"); Add("TC Kimlik No", txtNationalId);
            txtSalary = Theme.CreateTextBox("0.00"); Add("Maaş (₺)", txtSalary);
            txtAddress = Theme.CreateTextBox("Adres"); Add("Adres", txtAddress);
            txtCity = Theme.CreateTextBox("Şehir"); Add("Şehir", txtCity);
            txtEmergencyContact = Theme.CreateTextBox("Acil durumda aranacak kişi"); Add("Acil İrtibat", txtEmergencyContact);
            txtEmergencyPhone = Theme.CreateTextBox("Acil telefon"); Add("Acil Telefon", txtEmergencyPhone);

            cmbStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont };
            cmbStatus.Items.AddRange(new[] { "aktif", "izinli", "pasif", "ayrildi" });
            cmbStatus.SelectedIndex = 0;
            Add("Durum", cmbStatus);

            txtNotes = Theme.CreateTextBox("Notlar"); Add("Notlar", txtNotes);

            var btnSave = Theme.CreateButton("💾  Kaydet", Theme.Success, 220, 44);
            btnSave.Location = new Point(leftMargin, y); btnSave.Click += BtnSave_Click;
            var btnCancel = Theme.CreateButton("İptal", Color.Gray, 220, 44);
            btnCancel.Location = new Point(leftMargin + 240, y); btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            panel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            if (emp != null)
            {
                txtName.Text = emp.FullName ?? ""; txtEmail.Text = emp.Email ?? "";
                txtPhone.Text = emp.Phone ?? ""; txtDept.Text = emp.Department;
                txtPos.Text = emp.Position; dtpHire.Value = emp.HireDate;
                if (emp.BirthDate.HasValue) { dtpBirth.Checked = true; dtpBirth.Value = emp.BirthDate.Value; }
                txtNationalId.Text = emp.NationalId ?? "";
                txtSalary.Text = emp.Salary?.ToString("F2") ?? "";
                txtAddress.Text = emp.Address ?? ""; txtCity.Text = emp.City ?? "";
                txtEmergencyContact.Text = emp.EmergencyContact ?? "";
                txtEmergencyPhone.Text = emp.EmergencyPhone ?? "";
                txtNotes.Text = emp.Notes ?? "";
                var si = cmbStatus.Items.IndexOf(emp.Status);
                if (si >= 0) cmbStatus.SelectedIndex = si;
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtDept.Text) ||
                string.IsNullOrWhiteSpace(txtPos.Text))
            {
                MessageBox.Show("Zorunlu alanları doldurunuz."); return;
            }

            var emp = _emp ?? new Employee();
            emp.FullName = txtName.Text.Trim(); emp.Email = txtEmail.Text.Trim();
            emp.Phone = txtPhone.Text; emp.Department = txtDept.Text.Trim();
            emp.Position = txtPos.Text.Trim(); emp.HireDate = dtpHire.Value;
            emp.BirthDate = dtpBirth.Checked ? dtpBirth.Value : null;
            emp.NationalId = txtNationalId.Text; emp.Address = txtAddress.Text;
            emp.City = txtCity.Text; emp.EmergencyContact = txtEmergencyContact.Text;
            emp.EmergencyPhone = txtEmergencyPhone.Text;
            emp.Salary = decimal.TryParse(txtSalary.Text, out var sal) ? sal : null;
            emp.Status = cmbStatus.SelectedItem?.ToString() ?? "aktif";
            emp.Notes = txtNotes.Text;

            try
            {
                await SupabaseService.SaveEmployeeAsync(emp, txtPassword?.Text);
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }

    public class LeaveRequestsPage : UserControl
    {
        private DataGridView dgv = null!;

        public LeaveRequestsPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56, FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4, 8, 4, 8), BackColor = Theme.Surface
            };

            var btnAdd = Theme.CreateButton("+ Yeni İzin Talebi", Theme.Primary, 180);
            btnAdd.Click += BtnAdd_Click;

            var btnRefresh = Theme.CreateButton("🔄 Yenile", Theme.Info, 100);
            btnRefresh.Click += (s, e) => LoadData();

            toolbar.Controls.AddRange(new Control[] { btnAdd, btnRefresh });
            Controls.Add(toolbar);

            dgv = Theme.CreateDataGrid();
            dgv.Dock = DockStyle.Fill;
            Controls.Add(dgv);
            dgv.BringToFront(); toolbar.BringToFront();

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var leaves = await SupabaseService.GetLeaveRequestsAsync();
                dgv.Columns.Clear(); dgv.Rows.Clear();
                dgv.Columns.Add("Employee", "Personel");
                dgv.Columns.Add("Type", "İzin Tipi");
                dgv.Columns.Add("Start", "Başlangıç");
                dgv.Columns.Add("End", "Bitiş");
                dgv.Columns.Add("Days", "Gün");
                dgv.Columns.Add("Status", "Durum");
                dgv.Columns.Add("Reason", "Neden");

                foreach (var l in leaves)
                {
                    var typeText = l.LeaveType switch
                    {
                        "yillik" => "🏖 Yıllık",
                        "mazeret" => "📋 Mazeret",
                        "hastalik" => "🏥 Hastalık",
                        "dogum" => "👶 Doğum",
                        "ucretsiz" => "💰 Ücretsiz",
                        _ => l.LeaveType
                    };
                    var statusIcon = l.Status switch
                    {
                        "beklemede" => "⏳",
                        "onaylandi" => "✅",
                        "reddedildi" => "❌",
                        _ => ""
                    };
                    dgv.Rows.Add(l.EmployeeName ?? "-", typeText,
                        l.StartDate.ToString("dd.MM.yyyy"), l.EndDate.ToString("dd.MM.yyyy"),
                        l.TotalDays, $"{statusIcon} {l.Status}", l.Reason ?? "-");
                }
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            var employees = await SupabaseService.GetEmployeesAsync();
            var form = new LeaveRequestForm(employees);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }
    }

    public class LeaveRequestForm : Form
    {
        private ComboBox cmbEmployee = null!, cmbType = null!;
        private DateTimePicker dtpStart = null!, dtpEnd = null!;
        private TextBox txtReason = null!;
        private List<Employee> employees;

        public LeaveRequestForm(List<Employee> emps)
        {
            employees = emps;
            Text = "Yeni İzin Talebi";
            Size = new Size(450, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 10, 30, 10) };
            Controls.Add(panel);

            var header = Theme.CreateModernHeader("📋", "İzin Talebi");
            Controls.Add(header);
            int y = 10;
            int leftMargin = 15;

            void Add(string label, Control ctrl)
            {
                panel.Controls.Add(new Label { Text = label, Font = Theme.BodyFont, Location = new Point(leftMargin, y), AutoSize = true });
                ctrl.Location = new Point(leftMargin, y + 22); ctrl.Width = 370; panel.Controls.Add(ctrl); y += 55;
            }

            cmbEmployee = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont };
            foreach (var emp in employees) cmbEmployee.Items.Add($"{emp.EmployeeNo} - {emp.FullName}");
            if (cmbEmployee.Items.Count > 0) cmbEmployee.SelectedIndex = 0;
            Add("Personel *", cmbEmployee);

            cmbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont };
            cmbType.Items.AddRange(new[] { "yillik", "mazeret", "hastalik", "dogum", "ucretsiz" });
            cmbType.SelectedIndex = 0;
            Add("İzin Tipi *", cmbType);

            dtpStart = new DateTimePicker { Font = Theme.BodyFont, Format = DateTimePickerFormat.Short };
            Add("Başlangıç *", dtpStart);

            dtpEnd = new DateTimePicker { Font = Theme.BodyFont, Format = DateTimePickerFormat.Short };
            Add("Bitiş *", dtpEnd);

            txtReason = Theme.CreateTextBox("İzin nedeni"); Add("Neden", txtReason);

            var btnSave = Theme.CreateButton("💾  Kaydet", Theme.Success, 175, 44);
            btnSave.Location = new Point(leftMargin, y); btnSave.Click += BtnSave_Click;
            var btnCancel = Theme.CreateButton("İptal", Color.Gray, 175, 44);
            btnCancel.Location = new Point(leftMargin + 195, y); btnCancel.Click += (s, e2) => { DialogResult = DialogResult.Cancel; Close(); };
            panel.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cmbEmployee.SelectedIndex < 0) return;
            var days = (dtpEnd.Value.Date - dtpStart.Value.Date).Days + 1;
            if (days <= 0) { MessageBox.Show("Bitiş tarihi başlangıçtan sonra olmalıdır."); return; }

            var leave = new LeaveRequest
            {
                EmployeeId = employees[cmbEmployee.SelectedIndex].Id,
                LeaveType = cmbType.SelectedItem?.ToString() ?? "yillik",
                StartDate = dtpStart.Value.Date, EndDate = dtpEnd.Value.Date,
                TotalDays = days, Reason = txtReason.Text
            };

            try
            {
                await SupabaseService.SaveLeaveRequestAsync(leave);
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }
}
