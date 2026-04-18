using System;
using System.Drawing;
using System.Windows.Forms;
using HuzurMobilya.Models;
using HuzurMobilya.Services;

namespace HuzurMobilya.Forms
{
    public class ProductsPage : UserControl
    {
        private DataGridView dgv = null!;
        private TextBox txtSearch = null!;
        private List<Product> allProducts = new();

        public ProductsPage()
        {
            BackColor = Theme.Background;
            Dock = DockStyle.Fill;
            InitUI();
            LoadData();
        }

        private void InitUI()
        {
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 56,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4, 8, 4, 8),
                BackColor = Theme.Surface
            };

            txtSearch = Theme.CreateTextBox("🔍 Ürün ara...", 300);
            txtSearch.TextChanged += (s, e) => FilterProducts();

            var btnAdd = Theme.CreateButton("+ Yeni Ürün", Theme.Primary);
            btnAdd.Click += BtnAdd_Click;

            var btnRefresh = Theme.CreateButton("🔄 Yenile", Theme.Info, 100);
            btnRefresh.Click += (s, e) => LoadData();

            var btnDelete = Theme.CreateButton("🗑 Sil", Theme.Danger, 80);
            btnDelete.Click += BtnDelete_Click;

            toolbar.Controls.AddRange(new Control[] { txtSearch, btnAdd, btnRefresh, btnDelete });
            Controls.Add(toolbar);

            dgv = Theme.CreateDataGrid();
            dgv.Dock = DockStyle.Fill;
            dgv.CellDoubleClick += Dgv_CellDoubleClick;
            Controls.Add(dgv);

            // Ensure grid is behind toolbar
            dgv.BringToFront();
            toolbar.BringToFront();
        }

        private async void LoadData()
        {
            try
            {
                allProducts = await SupabaseService.GetProductsAsync();
                BindGrid(allProducts);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void BindGrid(List<Product> list)
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();
            dgv.Columns.Add("Sku", "SKU");
            dgv.Columns.Add("Name", "Ürün Adı");
            dgv.Columns.Add("Category", "Kategori");
            dgv.Columns.Add("Supplier", "Tedarikçi");
            dgv.Columns.Add("PurchasePrice", "Alış ₺");
            dgv.Columns.Add("SalePrice", "Satış ₺");
            dgv.Columns.Add("TaxRate", "KDV %");
            dgv.Columns.Add("Color", "Renk");
            dgv.Columns.Add("Material", "Malzeme");
            dgv.Columns.Add("MinStock", "Min Stok");
            dgv.Columns.Add("Active", "Aktif");

            foreach (var p in list)
            {
                dgv.Rows.Add(p.Sku, p.Name, p.CategoryName ?? "-", p.SupplierName ?? "-",
                    $"{p.PurchasePrice:N2}", $"{p.SalePrice:N2}", $"{p.TaxRate}",
                    p.Color ?? "-", p.Material ?? "-", p.MinStockLevel,
                    p.IsActive ? "✅" : "❌");
                dgv.Rows[^1].Tag = p;
            }
        }

        private void FilterProducts()
        {
            var q = txtSearch.Text.ToLower();
            var filtered = allProducts.FindAll(p =>
                p.Name.ToLower().Contains(q) || p.Sku.ToLower().Contains(q) ||
                (p.CategoryName?.ToLower().Contains(q) ?? false));
            BindGrid(filtered);
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var form = new ProductEditForm(null);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void Dgv_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var product = dgv.Rows[e.RowIndex].Tag as Product;
            if (product == null) return;
            var form = new ProductEditForm(product);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            var product = dgv.SelectedRows[0].Tag as Product;
            if (product == null) return;
            if (MessageBox.Show($"'{product.Name}' ürününü silmek istediğinize emin misiniz?",
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    await SupabaseService.DeleteProductAsync(product.Id);
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            }
        }
    }

    public class ProductEditForm : Form
    {
        private Product? _product;
        private TextBox txtSku = null!, txtName = null!, txtDesc = null!;
        private TextBox txtPurchase = null!, txtSale = null!, txtTax = null!;
        private TextBox txtColor = null!, txtMaterial = null!, txtMinStock = null!;
        private ComboBox cmbCategory = null!, cmbSupplier = null!;
        private CheckBox chkActive = null!;
        private PictureBox picImage = null!;
        private string? _selectedImagePath;
        private List<Category> categories = new();
        private List<Supplier> suppliers = new();

        public ProductEditForm(Product? product)
        {
            _product = product;
            InitializeComponent();
            LoadCombos();
        }

        private void InitializeComponent()
        {
            Text = _product == null ? "Yeni Ürün Ekle" : "Ürün Düzenle";
            Size = new Size(600, 760);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;

            // Panel ÖNCE eklenir (Z-sıralaması: arka)
            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Controls.Add(panel);

            // Header SONRA eklenir (Z-sıralaması: ön, Top'a dock'lanır)
            var headerPanel = Theme.CreateModernHeader("📦", _product == null ? "Yeni Ürün" : "Ürün Düzenle");
            Controls.Add(headerPanel);

            int y = 14;
            int leftCol = 20;
            int fieldWidth = 340;

            void AddField(string label, Control ctrl)
            {
                var lbl = Theme.CreateLabel(label);
                lbl.Location = new Point(leftCol, y);
                panel.Controls.Add(lbl);
                ctrl.Location = new Point(leftCol, y + 22);
                ctrl.Width = fieldWidth;
                panel.Controls.Add(ctrl);
                y += 60;
            }

            // ── Resim kutusu (sağ kolon) ──
            picImage = new PictureBox
            {
                Size = new Size(168, 130),
                Location = new Point(385, 14),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(243, 244, 246),
                Cursor = Cursors.Hand
            };
            panel.Controls.Add(picImage);

            var lblImgHint = new Label
            {
                Text = "📷  Resim",
                Font = new Font("Segoe UI", 8),
                ForeColor = Theme.TextSecondary,
                AutoSize = true,
                Location = new Point(395, 148),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblImgHint);

            var btnPickImg = Theme.CreateButton("Resim Seç", Theme.Info, 168, 34);
            btnPickImg.Location = new Point(385, 168);
            btnPickImg.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnPickImg.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog
                {
                    Title = "Ürün Resmi Seç",
                    Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp;*.webp"
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = dlg.FileName;
                    try
                    {
                        picImage.Image?.Dispose();
                        picImage.Image = Image.FromFile(dlg.FileName);
                        lblImgHint.Text = "✅  Resim seçildi";
                        lblImgHint.ForeColor = Theme.Success;
                    }
                    catch { }
                }
            };
            panel.Controls.Add(btnPickImg);

            // Mevcut ürün resmi varsa yükle
            if (_product?.ImageUrl != null)
            {
                lblImgHint.Text = "✅  Mevcut resim";
                lblImgHint.ForeColor = Theme.Success;
                Task.Run(async () =>
                {
                    try
                    {
                        using var http = new System.Net.Http.HttpClient();
                        var bytes = await http.GetByteArrayAsync(_product.ImageUrl);
                        using var ms = new System.IO.MemoryStream(bytes);
                        var img = Image.FromStream(ms);
                        picImage.Invoke(() => picImage.Image = img);
                    }
                    catch { }
                });
            }

            // ── Form alanları (sol kolon) ──
            txtSku = Theme.CreateTextBox("Ör: HM-KLT-001"); AddField("SKU *", txtSku);
            txtName = Theme.CreateTextBox("Ürün adı"); AddField("Ürün Adı *", txtName);

            cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont };
            AddField("Kategori", cmbCategory);

            cmbSupplier = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.BodyFont };
            AddField("Tedarikçi", cmbSupplier);

            txtPurchase = Theme.CreateTextBox("0.00"); AddField("Alış Fiyatı (₺) *", txtPurchase);
            txtSale = Theme.CreateTextBox("0.00"); AddField("Satış Fiyatı (₺) *", txtSale);
            txtTax = Theme.CreateTextBox("18"); AddField("KDV Oranı (%)", txtTax);
            txtColor = Theme.CreateTextBox("Ör: Beyaz"); AddField("Renk", txtColor);
            txtMaterial = Theme.CreateTextBox("Ör: Meşe"); AddField("Malzeme", txtMaterial);
            txtMinStock = Theme.CreateTextBox("5"); AddField("Minimum Stok", txtMinStock);

            txtDesc = new TextBox { Multiline = true, Height = 58, Font = Theme.BodyFont, ScrollBars = ScrollBars.Vertical };
            AddField("Açıklama", txtDesc);

            chkActive = new CheckBox { Text = "Aktif", Checked = true, Font = Theme.BodyFont };
            chkActive.Location = new Point(leftCol, y);
            panel.Controls.Add(chkActive);
            y += 42;

            var btnSave = Theme.CreateButton("💾  Kaydet", Theme.Success, 220, 44);
            btnSave.Location = new Point(leftCol, y);
            btnSave.Click += BtnSave_Click;

            var btnCancel = Theme.CreateButton("İptal", Color.Gray, 220, 44);
            btnCancel.Location = new Point(leftCol + 230, y);
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            panel.Controls.AddRange(new Control[] { btnSave, btnCancel });
            y += 58;
            panel.AutoScrollMinSize = new Size(0, y);

            if (_product != null)
            {
                txtSku.Text = _product.Sku;
                txtName.Text = _product.Name;
                txtPurchase.Text = _product.PurchasePrice.ToString("F2");
                txtSale.Text = _product.SalePrice.ToString("F2");
                txtTax.Text = _product.TaxRate.ToString("F0");
                txtColor.Text = _product.Color ?? "";
                txtMaterial.Text = _product.Material ?? "";
                txtMinStock.Text = _product.MinStockLevel.ToString();
                txtDesc.Text = _product.Description ?? "";
                chkActive.Checked = _product.IsActive;
            }
        }

        private async void LoadCombos()
        {
            try
            {
                categories = await SupabaseService.GetCategoriesAsync();
                suppliers = await SupabaseService.GetSuppliersAsync();

                cmbCategory.Items.Clear();
                cmbCategory.Items.Add("-- Seçiniz --");
                foreach (var c in categories) cmbCategory.Items.Add(c.Name);
                cmbCategory.SelectedIndex = 0;

                cmbSupplier.Items.Clear();
                cmbSupplier.Items.Add("-- Seçiniz --");
                foreach (var s in suppliers) cmbSupplier.Items.Add(s.CompanyName);
                cmbSupplier.SelectedIndex = 0;

                if (_product != null)
                {
                    var ci = categories.FindIndex(c => c.Id == _product.CategoryId);
                    if (ci >= 0) cmbCategory.SelectedIndex = ci + 1;
                    var si = suppliers.FindIndex(s => s.Id == _product.SupplierId);
                    if (si >= 0) cmbSupplier.SelectedIndex = si + 1;
                }
            }
            catch { }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSku.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("SKU ve Ürün Adı zorunludur.");
                return;
            }

            var product = _product ?? new Product();
            product.Sku = txtSku.Text.Trim();
            product.Name = txtName.Text.Trim();
            product.Description = txtDesc.Text;
            product.PurchasePrice = decimal.TryParse(txtPurchase.Text, out var pp) ? pp : 0;
            product.SalePrice = decimal.TryParse(txtSale.Text, out var sp) ? sp : 0;
            product.TaxRate = decimal.TryParse(txtTax.Text, out var tr) ? tr : 18;
            product.Color = txtColor.Text;
            product.Material = txtMaterial.Text;
            product.MinStockLevel = int.TryParse(txtMinStock.Text, out var ms) ? ms : 5;
            product.IsActive = chkActive.Checked;

            if (cmbCategory.SelectedIndex > 0)
                product.CategoryId = categories[cmbCategory.SelectedIndex - 1].Id;
            if (cmbSupplier.SelectedIndex > 0)
                product.SupplierId = suppliers[cmbSupplier.SelectedIndex - 1].Id;

            try
            {
                if (_selectedImagePath != null)
                {
                    var btnSaveCtrl = (Button)sender!;
                    btnSaveCtrl.Enabled = false;
                    btnSaveCtrl.Text = "⬆ Yükleniyor...";
                    try { product.ImageUrl = await SupabaseService.UploadImageAsync(_selectedImagePath); }
                    catch { /* resim yuklenemedi, devam et */ }
                    btnSaveCtrl.Enabled = true;
                    btnSaveCtrl.Text = "💾  Kaydet";
                }
                await SupabaseService.SaveProductAsync(product);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
    }
}
