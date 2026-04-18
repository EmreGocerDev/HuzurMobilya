using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HuzurMobilya.Services;

namespace HuzurMobilya.Forms
{
    public class DashboardPage : UserControl
    {
        public DashboardPage()
        {
            BackColor = Theme.Background;
            Dock = DockStyle.Fill;
            AutoScroll = true;
            LoadDashboard();
        }

        private async void LoadDashboard()
        {
            var lblWelcome = new Label
            {
                Text = $"Hoş geldiniz, {SupabaseService.CurrentUser?.FullName ?? "Kullanıcı"}! 👋",
                Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Theme.TextPrimary,
                AutoSize = true, Location = new Point(10, 10)
            };

            var lblDate = new Label
            {
                Text = DateTime.Now.ToString("dd MMMM yyyy, dddd"),
                Font = new Font("Segoe UI", 10), ForeColor = Theme.TextSecondary,
                AutoSize = true, Location = new Point(12, 50)
            };

            Controls.AddRange(new Control[] { lblWelcome, lblDate });

            try
            {
                var productsTask = SupabaseService.GetProductsAsync();
                var employeesTask = SupabaseService.GetEmployeesAsync();
                var ordersTask = SupabaseService.GetOrdersAsync();
                var customersTask = SupabaseService.GetCustomersAsync();
                var stockTask = SupabaseService.GetStockAsync();
                await Task.WhenAll(productsTask, employeesTask, ordersTask, customersTask, stockTask);
                var products = productsTask.Result;
                var employees = employeesTask.Result;
                var orders = ordersTask.Result;
                var customers = customersTask.Result;
                var stock = stockTask.Result;

                int lowStock = 0;
                foreach (var s in stock)
                {
                    var p = products.Find(pr => pr.Id == s.ProductId);
                    if (p != null && s.Quantity <= p.MinStockLevel) lowStock++;
                }

                decimal monthlyRev = 0;
                int monthlyOrd = 0;
                foreach (var o in orders)
                {
                    if (o.CreatedAt.Month == DateTime.Now.Month && o.CreatedAt.Year == DateTime.Now.Year)
                    {
                        monthlyRev += o.GrandTotal;
                        monthlyOrd++;
                    }
                }

                int y = 85;
                var cardsPanel = new FlowLayoutPanel
                {
                    Location = new Point(10, y), Size = new Size(1120, 150),
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false, AutoScroll = true,
                    BackColor = Color.Transparent
                };

                cardsPanel.Controls.Add(Theme.CreateStatCard("Toplam Ürün", products.Count.ToString(), "📦", Theme.Primary, 190));
                cardsPanel.Controls.Add(Theme.CreateStatCard("Düşük Stok", lowStock.ToString(), "⚠️", Theme.Warning, 190));
                cardsPanel.Controls.Add(Theme.CreateStatCard("Aktif Personel", employees.FindAll(e => e.Status == "aktif").Count.ToString(), "👨‍💼", Theme.Success, 190));
                cardsPanel.Controls.Add(Theme.CreateStatCard("Aylık Sipariş", monthlyOrd.ToString(), "🛒", Theme.Info, 190));
                cardsPanel.Controls.Add(Theme.CreateStatCard("Aylık Gelir", $"₺{monthlyRev:N0}", "💰", Theme.Success, 200));
                cardsPanel.Controls.Add(Theme.CreateStatCard("Müşteriler", customers.Count.ToString(), "👥", Theme.Accent, 190));

                Controls.Add(cardsPanel);

                // Recent orders section
                y = 240;
                var lblRecent = new Label
                {
                    Text = "📋  Son Siparişler", Font = new Font("Segoe UI", 13, FontStyle.Bold),
                    ForeColor = Theme.TextPrimary, AutoSize = true, Location = new Point(10, y)
                };
                Controls.Add(lblRecent);

                var dgv = Theme.CreateDataGrid();
                dgv.Location = new Point(10, y + 35);
                dgv.Size = new Size(1100, 250);

                dgv.Columns.Add("OrderNo", "Sipariş No");
                dgv.Columns.Add("Customer", "Müşteri");
                dgv.Columns.Add("Total", "Tutar");
                dgv.Columns.Add("Status", "Durum");
                dgv.Columns.Add("Date", "Tarih");

                var recent = orders.Count > 10 ? orders.GetRange(0, 10) : orders;
                foreach (var o in recent)
                {
                    dgv.Rows.Add(o.OrderNo, o.CustomerName ?? "-",
                        $"₺{o.GrandTotal:N2}", o.OrderStatus, o.CreatedAt.ToString("dd.MM.yyyy"));
                }

                Controls.Add(dgv);

                // Low stock warning section
                var lblLow = new Label
                {
                    Text = "⚠️  Düşük Stok Uyarıları", Font = new Font("Segoe UI", 13, FontStyle.Bold),
                    ForeColor = Theme.Warning, AutoSize = true, Location = new Point(10, y + 300)
                };
                Controls.Add(lblLow);

                var dgvLow = Theme.CreateDataGrid();
                dgvLow.Location = new Point(10, y + 335);
                dgvLow.Size = new Size(1100, 200);

                dgvLow.Columns.Add("Sku", "SKU");
                dgvLow.Columns.Add("Product", "Ürün");
                dgvLow.Columns.Add("Stock", "Mevcut Stok");
                dgvLow.Columns.Add("Min", "Min Stok");
                dgvLow.Columns.Add("Warehouse", "Depo");

                foreach (var s in stock)
                {
                    var p = products.Find(pr => pr.Id == s.ProductId);
                    if (p != null && s.Quantity <= p.MinStockLevel)
                    {
                        dgvLow.Rows.Add(p.Sku, p.Name, s.Quantity, p.MinStockLevel, s.WarehouseName ?? "-");
                    }
                }

                Controls.Add(dgvLow);
            }
            catch (Exception ex)
            {
                var lblErr = new Label
                {
                    Text = "Veri yüklenirken hata: " + ex.Message,
                    ForeColor = Theme.Danger, Font = Theme.BodyFont,
                    AutoSize = true, Location = new Point(10, 90)
                };
                Controls.Add(lblErr);
            }
        }
    }
}
