using System;
using System.Drawing;
using System.Windows.Forms;
using HuzurMobilya.Models;
using HuzurMobilya.Services;

namespace HuzurMobilya.Forms
{
    public class StockPage : UserControl
    {
        private DataGridView dgv = null!;
        private TextBox txtSearch = null!;
        private List<StockItem> allStock = new();

        public StockPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56,
                FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(4, 8, 4, 8),
                BackColor = Theme.Surface
            };

            txtSearch = Theme.CreateTextBox("🔍 Stok ara...", 300);
            txtSearch.TextChanged += (s, e) => Filter();

            var btnRefresh = Theme.CreateButton("🔄 Yenile", Theme.Info, 100);
            btnRefresh.Click += (s, e) => LoadData();

            toolbar.Controls.AddRange(new Control[] { txtSearch, btnRefresh });
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
                allStock = await SupabaseService.GetStockAsync();
                BindGrid(allStock);
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void BindGrid(List<StockItem> list)
        {
            dgv.Columns.Clear(); dgv.Rows.Clear();
            dgv.Columns.Add("Sku", "SKU");
            dgv.Columns.Add("Product", "Ürün");
            dgv.Columns.Add("Warehouse", "Depo");
            dgv.Columns.Add("Qty", "Miktar");
            dgv.Columns.Add("Reserved", "Rezerve");
            dgv.Columns.Add("Available", "Kullanılabilir");

            foreach (var s in list)
            {
                dgv.Rows.Add(s.Sku ?? "-", s.ProductName ?? "-", s.WarehouseName ?? "-",
                    s.Quantity, s.ReservedQuantity, s.Quantity - s.ReservedQuantity);
            }
        }

        private void Filter()
        {
            var q = txtSearch.Text.ToLower();
            BindGrid(allStock.FindAll(s =>
                (s.ProductName?.ToLower().Contains(q) ?? false) ||
                (s.Sku?.ToLower().Contains(q) ?? false) ||
                (s.WarehouseName?.ToLower().Contains(q) ?? false)));
        }
    }

    public class StockMovementsPage : UserControl
    {
        private DataGridView dgv = null!;

        public StockMovementsPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56,
                FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(4, 8, 4, 8),
                BackColor = Theme.Surface
            };

            var btnAdd = Theme.CreateButton("+ Stok Hareketi", Theme.Primary, 160);
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
                var movements = await SupabaseService.GetStockMovementsAsync();
                dgv.Columns.Clear(); dgv.Rows.Clear();
                dgv.Columns.Add("Date", "Tarih");
                dgv.Columns.Add("Type", "Hareket");
                dgv.Columns.Add("Product", "Ürün");
                dgv.Columns.Add("Warehouse", "Depo");
                dgv.Columns.Add("Qty", "Miktar");
                dgv.Columns.Add("Price", "Birim Fiyat");
                dgv.Columns.Add("Ref", "Referans No");
                dgv.Columns.Add("Notes", "Notlar");

                foreach (var m in movements)
                {
                    var typeText = m.MovementType switch
                    {
                        "giris" => "📥 Giriş",
                        "cikis" => "📤 Çıkış",
                        "transfer" => "🔄 Transfer",
                        "iade" => "↩️ İade",
                        "fire" => "🔥 Fire",
                        _ => m.MovementType
                    };
                    dgv.Rows.Add(m.CreatedAt.ToString("dd.MM.yyyy HH:mm"), typeText,
                        m.ProductName ?? "-", m.WarehouseName ?? "-", m.Quantity,
                        m.UnitPrice.HasValue ? $"₺{m.UnitPrice:N2}" : "-",
                        m.ReferenceNo ?? "-", m.Notes ?? "-");
                }
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var form = new StockMovementForm();
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }
    }

    public class StockMovementForm : Form
    {
        private ComboBox cmbProduct = null!, cmbWarehouse = null!, cmbType = null!;
        private TextBox txtQty = null!, txtPrice = null!, txtRef = null!, txtNotes = null!;
        private List<Product> products = new();
        private List<Warehouse> warehouses = new();

        public StockMovementForm()
        {
            Text = "Yeni Stok Hareketi";
            Size = new Size(480, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;

            // Panel ÖNCE (arka Z), header SONRA (ön Z - Top'a yapışır)
            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Controls.Add(panel);

            var header = Theme.CreateModernHeader("🔄", "Stok Hareketi Ekle");
            Controls.Add(header);

            int y = 14;
            int lm = 20;

            void Add(string label, Control ctrl)
            {
                panel.Controls.Add(new Label { Text = label, Font = Theme.BodyFont, Location = new Point(lm, y), AutoSize = true });
                ctrl.Location = new Point(lm, y + 22); ctrl.Width = 420;
                panel.Controls.Add(ctrl); y += 60;
            }

            cmbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont };
            cmbType.Items.AddRange(new[] { "📥 Giriş", "📤 Çıkış", "🔄 Transfer", "↩️ İade", "🔥 Fire" });
            cmbType.SelectedIndex = 0;
            Add("Hareket Tipi *", cmbType);

            cmbProduct = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont };
            Add("Ürün *", cmbProduct);

            cmbWarehouse = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont };
            Add("Depo *", cmbWarehouse);

            txtQty = Theme.CreateTextBox("Adet"); Add("Miktar *", txtQty);
            txtPrice = Theme.CreateTextBox("0.00"); Add("Birim Fiyat (₺)", txtPrice);
            txtRef = Theme.CreateTextBox("Referans numarası"); Add("Referans No", txtRef);
            txtNotes = Theme.CreateTextBox("Notlar"); Add("Notlar", txtNotes);

            var btnSave = Theme.CreateButton("💾  Kaydet", Theme.Success, 200, 44);
            btnSave.Location = new Point(lm, y);
            btnSave.Click += BtnSave_Click;

            var btnCancel = Theme.CreateButton("İptal", Color.Gray, 200, 44);
            btnCancel.Location = new Point(lm + 216, y);
            btnCancel.Click += (s, e2) => { DialogResult = DialogResult.Cancel; Close(); };

            panel.Controls.AddRange(new Control[] { btnSave, btnCancel });
            y += 56;
            panel.AutoScrollMinSize = new Size(0, y);

            LoadCombos();
        }

        private async void LoadCombos()
        {
            try
            {
                products = await SupabaseService.GetProductsAsync();
                warehouses = await SupabaseService.GetWarehousesAsync();

                cmbProduct.Items.Clear();
                foreach (var p in products) cmbProduct.Items.Add($"{p.Sku} - {p.Name}");
                if (cmbProduct.Items.Count > 0) cmbProduct.SelectedIndex = 0;

                cmbWarehouse.Items.Clear();
                foreach (var w in warehouses) cmbWarehouse.Items.Add(w.Name);
                if (cmbWarehouse.Items.Count > 0) cmbWarehouse.SelectedIndex = 0;
            }
            catch { }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cmbProduct.SelectedIndex < 0 || cmbWarehouse.SelectedIndex < 0 ||
                !int.TryParse(txtQty.Text, out var qty) || qty <= 0)
            {
                MessageBox.Show("Lütfen tüm zorunlu alanları doldurunuz.");
                return;
            }

            var types = new[] { "giris", "cikis", "transfer", "iade", "fire" };
            var movement = new StockMovement
            {
                ProductId = products[cmbProduct.SelectedIndex].Id,
                WarehouseId = warehouses[cmbWarehouse.SelectedIndex].Id,
                MovementType = types[cmbType.SelectedIndex],
                Quantity = qty,
                UnitPrice = decimal.TryParse(txtPrice.Text, out var up) ? up : null,
                ReferenceNo = txtRef.Text, Notes = txtNotes.Text
            };

            try
            {
                await SupabaseService.AddStockMovementAsync(movement);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }
}
