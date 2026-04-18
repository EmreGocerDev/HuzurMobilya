using System;
using System.Drawing;
using System.Windows.Forms;
using HuzurMobilya.Models;
using HuzurMobilya.Services;

namespace HuzurMobilya.Forms
{
    public class OrdersPage : UserControl
    {
        private DataGridView dgv = null!;

        public OrdersPage()
        {
            BackColor = Theme.Background; Dock = DockStyle.Fill;

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56, FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4, 8, 4, 8), BackColor = Theme.Surface
            };

            var btnAdd = Theme.CreateButton("+ Yeni Sipariş", Theme.Primary, 160);
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
                var orders = await SupabaseService.GetOrdersAsync();
                dgv.Columns.Clear(); dgv.Rows.Clear();
                dgv.Columns.Add("No", "Sipariş No");
                dgv.Columns.Add("Customer", "Müşteri");
                dgv.Columns.Add("Total", "Toplam");
                dgv.Columns.Add("OStatus", "Sipariş Durumu");
                dgv.Columns.Add("PStatus", "Ödeme Durumu");
                dgv.Columns.Add("Date", "Tarih");

                foreach (var o in orders)
                {
                    var statusIcon = o.OrderStatus switch
                    {
                        "beklemede" => "⏳",
                        "hazirlaniyor" => "🔧",
                        "kargoda" => "🚚",
                        "teslim_edildi" => "✅",
                        "iptal" => "❌",
                        _ => ""
                    };
                    dgv.Rows.Add(o.OrderNo, o.CustomerName ?? "-", $"₺{o.GrandTotal:N2}",
                        $"{statusIcon} {o.OrderStatus}", o.PaymentStatus, o.CreatedAt.ToString("dd.MM.yyyy"));
                    dgv.Rows[^1].Tag = o;
                }
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var form = new OrderCreateForm();
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }
    }

    public class OrderCreateForm : Form
    {
        private ComboBox cmbCustomer = null!;
        private DataGridView dgvItems = null!;
        private ComboBox cmbProduct = null!;
        private TextBox txtQty = null!, txtNotes = null!;
        private Label lblTotal = null!;
        private List<Customer> customers = new();
        private List<Product> products = new();
        private List<OrderItem> items = new();

        public OrderCreateForm()
        {
            Text = "Yeni Sipariş Oluştur";
            Size = new Size(760, 700);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;

            // Panel ÖNCE (arka Z), header SONRA (ön Z - Top'a yapışır)
            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Controls.Add(panel);

            var header = Theme.CreateModernHeader("🛒", "Yeni Sipariş");
            Controls.Add(header);

            int lm = 20;
            int cw = 700; // content width

            // ── Müşteri seçimi ──
            panel.Controls.Add(new Label { Text = "Müşteri *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(lm, 14) });
            cmbCustomer = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont, Width = 400, Location = new Point(lm, 36) };
            panel.Controls.Add(cmbCustomer);

            // ── Ürün ekleme satırı ──
            panel.Controls.Add(new Label { Text = "Ürün Ekle", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(lm, 82) });
            cmbProduct = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont, Width = 380, Location = new Point(lm, 104) };
            panel.Controls.Add(cmbProduct);

            panel.Controls.Add(new Label { Text = "Adet", Font = Theme.BodyFont, ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(412, 82) });
            txtQty = Theme.CreateTextBox("1", 80); txtQty.Location = new Point(412, 104);
            panel.Controls.Add(txtQty);

            var btnAddItem = Theme.CreateButton("+ Ekle", Theme.Success, 110, 38);
            btnAddItem.Location = new Point(502, 102); btnAddItem.Click += BtnAddItem_Click;
            panel.Controls.Add(btnAddItem);

            // ── Ürün listesi grid ──
            dgvItems = Theme.CreateDataGrid();
            dgvItems.Location = new Point(lm, 154); dgvItems.Size = new Size(cw, 220);
            dgvItems.Columns.Add("Product", "Ürün");
            dgvItems.Columns.Add("Qty", "Adet");
            dgvItems.Columns.Add("Price", "Birim Fiyat");
            dgvItems.Columns.Add("Tax", "KDV %");
            dgvItems.Columns.Add("Total", "Toplam");
            panel.Controls.Add(dgvItems);

            lblTotal = new Label
            {
                Text = "Genel Toplam: ₺0.00", Font = Theme.SubtitleFont,
                ForeColor = Theme.Primary, AutoSize = true, Location = new Point(lm, 385)
            };
            panel.Controls.Add(lblTotal);

            panel.Controls.Add(new Label { Text = "Notlar", Font = Theme.BodyFont, ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(lm, 420) });
            txtNotes = new TextBox { Multiline = true, Height = 52, Width = cw, Location = new Point(lm, 442), Font = Theme.BodyFont, ScrollBars = ScrollBars.Vertical };
            panel.Controls.Add(txtNotes);

            var btnSave = Theme.CreateButton("💾  Sipariş Oluştur", Theme.Success, 230, 44);
            btnSave.Location = new Point(lm, 506); btnSave.Click += BtnSave_Click;
            var btnCancel = Theme.CreateButton("İptal", Color.Gray, 220, 44);
            btnCancel.Location = new Point(lm + 246, 506); btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            panel.Controls.AddRange(new Control[] { btnSave, btnCancel });
            panel.AutoScrollMinSize = new Size(0, 562);

            LoadCombos();
        }

        private async void LoadCombos()
        {
            try
            {
                customers = await SupabaseService.GetCustomersAsync();
                products = await SupabaseService.GetProductsAsync();

                foreach (var c in customers) cmbCustomer.Items.Add(c.FullName);
                if (cmbCustomer.Items.Count > 0) cmbCustomer.SelectedIndex = 0;

                foreach (var p in products) cmbProduct.Items.Add($"{p.Sku} - {p.Name} (₺{p.SalePrice:N2})");
                if (cmbProduct.Items.Count > 0) cmbProduct.SelectedIndex = 0;
            }
            catch { }
        }

        private void BtnAddItem_Click(object? sender, EventArgs e)
        {
            if (cmbProduct.SelectedIndex < 0 || !int.TryParse(txtQty.Text, out var qty) || qty <= 0) return;

            var product = products[cmbProduct.SelectedIndex];
            var lineTotal = product.SalePrice * qty * (1 + product.TaxRate / 100);

            var item = new OrderItem
            {
                ProductId = product.Id, ProductName = product.Name, Sku = product.Sku,
                Quantity = qty, UnitPrice = product.SalePrice, TaxRate = product.TaxRate,
                LineTotal = lineTotal
            };
            items.Add(item);

            dgvItems.Rows.Add(product.Name, qty, $"₺{product.SalePrice:N2}", $"{product.TaxRate}%", $"₺{lineTotal:N2}");
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (var i in items) total += i.LineTotal;
            lblTotal.Text = $"Genel Toplam: ₺{total:N2}";
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cmbCustomer.SelectedIndex < 0 || items.Count == 0)
            {
                MessageBox.Show("Müşteri seçip en az 1 ürün ekleyiniz.");
                return;
            }

            decimal subtotal = 0, taxTotal = 0;
            foreach (var i in items)
            {
                subtotal += i.UnitPrice * i.Quantity;
                taxTotal += i.UnitPrice * i.Quantity * i.TaxRate / 100;
            }

            var order = new Order
            {
                CustomerId = customers[cmbCustomer.SelectedIndex].Id,
                Subtotal = subtotal, TaxTotal = taxTotal, GrandTotal = subtotal + taxTotal,
                Notes = txtNotes.Text
            };

            try
            {
                await SupabaseService.CreateOrderAsync(order, items);
                MessageBox.Show("Sipariş başarıyla oluşturuldu!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }
}
