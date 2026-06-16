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

            var lblHint = new Label
            {
                Text = "  ℹ Sağ tıklayın veya çift tıklayın → Durum/Ödeme güncelleyin",
                ForeColor = Theme.TextSecondary, Font = Theme.SmallFont,
                AutoSize = true, Margin = new Padding(10, 14, 0, 0)
            };

            toolbar.Controls.AddRange(new Control[] { btnAdd, btnRefresh, lblHint });
            Controls.Add(toolbar);

            dgv = Theme.CreateDataGrid();
            dgv.Dock = DockStyle.Fill;
            dgv.CellDoubleClick += DgvCellDoubleClick;

            // ── Sağ tık menüsü ──
            var ctx = new ContextMenuStrip();

            var mnuOrderStatus = new ToolStripMenuItem("📋 Sipariş Durumu");
            mnuOrderStatus.DropDownItems.Add("⏳ Beklemede",     null, (s, e) => UpdateSelectedOrderStatus("beklemede"));
            mnuOrderStatus.DropDownItems.Add("🔧 Hazırlanıyor",  null, (s, e) => UpdateSelectedOrderStatus("hazirlaniyor"));
            mnuOrderStatus.DropDownItems.Add("🚚 Kargoda",       null, (s, e) => UpdateSelectedOrderStatus("kargoda"));
            mnuOrderStatus.DropDownItems.Add("✅ Teslim Edildi", null, (s, e) => UpdateSelectedOrderStatus("teslim_edildi"));
            mnuOrderStatus.DropDownItems.Add("❌ İptal",         null, (s, e) => UpdateSelectedOrderStatus("iptal"));
            ctx.Items.Add(mnuOrderStatus);

            var mnuPayStatus = new ToolStripMenuItem("💰 Ödeme Durumu");
            mnuPayStatus.DropDownItems.Add("🔴 Ödenmedi",     null, (s, e) => UpdateSelectedPaymentStatus("odenmedi"));
            mnuPayStatus.DropDownItems.Add("🟡 Kısmi Ödendi", null, (s, e) => UpdateSelectedPaymentStatus("kismi_odendi"));
            mnuPayStatus.DropDownItems.Add("🟢 Ödendi",       null, (s, e) => UpdateSelectedPaymentStatus("odendi"));
            ctx.Items.Add(mnuPayStatus);

            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add("⚡ Hızlı Onayla (Hazırlanıyor + Ödendi)", null, (s, e) => QuickApprove());

            dgv.ContextMenuStrip = ctx;

            Controls.Add(dgv);
            dgv.BringToFront(); toolbar.BringToFront();

            LoadData();
        }

        private Order? GetSelectedOrder()
        {
            if (dgv.CurrentRow == null) return null;
            return dgv.CurrentRow.Tag as Order;
        }

        private async void UpdateSelectedOrderStatus(string newStatus)
        {
            var o = GetSelectedOrder();
            if (o == null) return;
            try
            {
                await SupabaseService.UpdateOrderStatusAsync(o.Id, newStatus, o.PaymentStatus);
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private async void UpdateSelectedPaymentStatus(string newPayStatus)
        {
            var o = GetSelectedOrder();
            if (o == null) return;
            try
            {
                await SupabaseService.UpdateOrderStatusAsync(o.Id, o.OrderStatus, newPayStatus);
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private async void QuickApprove()
        {
            var o = GetSelectedOrder();
            if (o == null) return;
            var confirm = MessageBox.Show(
                $"Sipariş #{o.OrderNo} durumunu 'Hazırlanıyor' ve ödemeyi 'Ödendi' olarak işaretlemek istiyor musunuz?",
                "Hızlı Onayla", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            try
            {
                await SupabaseService.UpdateOrderStatusAsync(o.Id, "hazirlaniyor", "odendi");
                MessageBox.Show("Sipariş onaylandı ve ödeme alındı olarak işaretlendi!", "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void DgvCellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var o = dgv.Rows[e.RowIndex].Tag as Order;
            if (o == null) return;
            var f = new OrderDetailForm(o);
            if (f.ShowDialog() == DialogResult.OK) LoadData();
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
                    var orderIcon = o.OrderStatus switch
                    {
                        "beklemede"     => "⏳ Beklemede",
                        "hazirlaniyor"  => "🔧 Hazırlanıyor",
                        "kargoda"       => "🚚 Kargoda",
                        "teslim_edildi" => "✅ Teslim Edildi",
                        "iptal"         => "❌ İptal",
                        _ => o.OrderStatus
                    };
                    var payIcon = o.PaymentStatus switch
                    {
                        "odendi"       => "🟢 Ödendi",
                        "kismi_odendi" => "🟡 Kısmi Ödendi",
                        "odenmedi"     => "🔴 Ödenmedi",
                        _ => o.PaymentStatus
                    };

                    int idx = dgv.Rows.Add(
                        o.OrderNo, o.CustomerName ?? "-",
                        $"₺{o.GrandTotal:N2}", orderIcon, payIcon,
                        o.CreatedAt.ToString("dd.MM.yyyy"));
                    dgv.Rows[idx].Tag = o;

                    if (o.PaymentStatus == "odendi")
                        dgv.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 248);
                    else if (o.OrderStatus == "iptal")
                        dgv.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 245);
                    else if (o.PaymentStatus == "odenmedi")
                        dgv.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(255, 252, 240);
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

    // ── Sipariş Detay / Durum Güncelle Formu ──
    public class OrderDetailForm : Form
    {
        private Order _order;
        private ComboBox cmbOrderStatus = null!, cmbPayStatus = null!;

        public OrderDetailForm(Order order)
        {
            _order = order;
            Text = $"Sipariş #{order.OrderNo} - Detay";
            Size = new Size(500, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;

            var header = Theme.CreateModernHeader("🛒", $"Sipariş #{order.OrderNo}");
            Controls.Add(header);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16) };
            Controls.Add(panel);
            panel.BringToFront(); header.BringToFront();

            int y = 20;
            void AddRow(string label, string value)
            {
                panel.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(0, y) });
                panel.Controls.Add(new Label { Text = value, Font = Theme.BodyFont, ForeColor = Theme.TextPrimary, AutoSize = true, Location = new Point(160, y) });
                y += 28;
            }

            AddRow("Müşteri:", order.CustomerName ?? "-");
            AddRow("Tutar:", $"₺{order.GrandTotal:N2}");
            AddRow("Oluşturulma:", order.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
            if (!string.IsNullOrEmpty(order.Notes)) AddRow("Notlar:", order.Notes);
            y += 10;

            panel.Controls.Add(new Label { Text = "Sipariş Durumu:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(0, y) });
            cmbOrderStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont, Width = 220, Location = new Point(160, y - 2) };
            cmbOrderStatus.Items.AddRange(new object[] { "beklemede", "hazirlaniyor", "kargoda", "teslim_edildi", "iptal" });
            cmbOrderStatus.Text = order.OrderStatus;
            panel.Controls.Add(cmbOrderStatus);
            y += 34;

            panel.Controls.Add(new Label { Text = "Ödeme Durumu:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Theme.TextSecondary, AutoSize = true, Location = new Point(0, y) });
            cmbPayStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont, Width = 220, Location = new Point(160, y - 2) };
            cmbPayStatus.Items.AddRange(new object[] { "odenmedi", "kismi_odendi", "odendi" });
            cmbPayStatus.Text = order.PaymentStatus;
            panel.Controls.Add(cmbPayStatus);
            y += 46;

            var btnSave = Theme.CreateButton("💾 Güncelle", Theme.Success, 160, 40);
            btnSave.Location = new Point(0, y);
            btnSave.Click += BtnSave_Click;
            panel.Controls.Add(btnSave);

            var btnCancel = Theme.CreateButton("İptal", Color.Gray, 120, 40);
            btnCancel.Location = new Point(170, y);
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            panel.Controls.Add(btnCancel);
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                await SupabaseService.UpdateOrderStatusAsync(_order.Id, cmbOrderStatus.Text, cmbPayStatus.Text);
                MessageBox.Show("Sipariş güncellendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
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
