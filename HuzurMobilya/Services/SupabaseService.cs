using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HuzurMobilya.Models;

namespace HuzurMobilya.Services
{
    public static class SupabaseService
    {
        private static HttpClient _http = new();
        private static string _url = "";
        private static string _key = "";
        public static Profile? CurrentUser { get; set; }

        public static async Task InitializeAsync()
        {
            var envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            if (!File.Exists(envPath))
                envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envPath))
                DotNetEnv.Env.Load(envPath);

            _url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? "";
            _key = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? "";

            if (string.IsNullOrEmpty(_url) || string.IsNullOrEmpty(_key))
                throw new Exception(".env dosyasinda SUPABASE_URL ve SUPABASE_KEY tanimli olmali.");

            _http.DefaultRequestHeaders.Add("apikey", _key);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _key);
            _http.DefaultRequestHeaders.Add("Prefer", "return=representation");

            var test = await _http.GetAsync($"{_url}/rest/v1/profiles?limit=1");
            if (!test.IsSuccessStatusCode)
                throw new Exception("Supabase baglantisi basarisiz: " + test.StatusCode);
        }

        private static async Task<List<T>> GetTable<T>(string table, string query = "")
        {
            var resp = await _http.GetStringAsync($"{_url}/rest/v1/{table}?{query}");
            return JsonConvert.DeserializeObject<List<T>>(resp) ?? new();
        }

        private static async Task<T?> Insert<T>(string table, object data)
        {
            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"{_url}/rest/v1/{table}", content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) throw new Exception(body);
            var list = JsonConvert.DeserializeObject<List<T>>(body);
            return list != null && list.Count > 0 ? list[0] : default;
        }

        private static async Task Update(string table, string id, object data)
        {
            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var req = new HttpRequestMessage(HttpMethod.Patch, $"{_url}/rest/v1/{table}?id=eq.{id}");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) throw new Exception(await resp.Content.ReadAsStringAsync());
        }

        private static async Task Delete(string table, string id)
        {
            var resp = await _http.DeleteAsync($"{_url}/rest/v1/{table}?id=eq.{id}");
            if (!resp.IsSuccessStatusCode) throw new Exception(await resp.Content.ReadAsStringAsync());
        }

        // ---- AUTH ----
        private static string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var salt = Guid.NewGuid().ToString("N").Substring(0, 16);
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(salt + password));
            return salt + ":" + Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            var parts = hash.Split(':');
            if (parts.Length != 2) return false;
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(parts[0] + password));
            return parts[1] == Convert.ToBase64String(bytes);
        }

        public static async Task<Profile?> LoginAsync(string email, string password)
        {
            var list = await GetTable<ProfileDto>("profiles", $"email=eq.{Uri.EscapeDataString(email)}");
            var row = list.FirstOrDefault();
            if (row == null) return null;
            if (!VerifyPassword(password, row.password_hash)) return null;
            CurrentUser = new Profile
            {
                Id = row.id, Email = row.email, FullName = row.full_name,
                Phone = row.phone, Role = row.role, IsActive = row.is_active
            };
            return CurrentUser;
        }

        public static async Task<bool> RegisterAsync(string fullName, string email, string password, string? phone)
        {
            await Insert<ProfileDto>("profiles", new
            {
                email, password_hash = HashPassword(password),
                full_name = fullName, phone, role = "employee"
            });
            return true;
        }

        // ---- CATEGORIES ----
        public static async Task<List<Category>> GetCategoriesAsync()
        {
            var rows = await GetTable<CategoryDto>("categories", "order=sort_order.asc");
            return rows.Select(x => new Category { Id = x.id, Name = x.name, Description = x.description, ParentId = x.parent_id, SortOrder = x.sort_order, IsActive = x.is_active }).ToList();
        }

        public static async Task SaveCategoryAsync(Category c)
        {
            var data = new { name = c.Name, description = c.Description, parent_id = c.ParentId, sort_order = c.SortOrder, is_active = c.IsActive };
            if (string.IsNullOrEmpty(c.Id)) await Insert<object>("categories", data);
            else await Update("categories", c.Id, data);
        }

        // ---- SUPPLIERS ----
        public static async Task<List<Supplier>> GetSuppliersAsync()
        {
            var rows = await GetTable<SupplierDto>("suppliers", "order=company_name.asc");
            return rows.Select(x => new Supplier { Id = x.id, CompanyName = x.company_name, ContactName = x.contact_name, Email = x.email, Phone = x.phone, Address = x.address, City = x.city, TaxNumber = x.tax_number, TaxOffice = x.tax_office, Notes = x.notes, IsActive = x.is_active }).ToList();
        }

        public static async Task SaveSupplierAsync(Supplier s)
        {
            var data = new { company_name = s.CompanyName, contact_name = s.ContactName, email = s.Email, phone = s.Phone, address = s.Address, city = s.City, tax_number = s.TaxNumber, tax_office = s.TaxOffice, notes = s.Notes, is_active = s.IsActive };
            if (string.IsNullOrEmpty(s.Id)) await Insert<object>("suppliers", data);
            else await Update("suppliers", s.Id, data);
        }

        // ---- WAREHOUSES ----
        public static async Task<List<Warehouse>> GetWarehousesAsync()
        {
            var rows = await GetTable<WarehouseDto>("warehouses", "");
            return rows.Select(x => new Warehouse { Id = x.id, Name = x.name, Address = x.address, City = x.city, Capacity = x.capacity, IsActive = x.is_active }).ToList();
        }

        // ---- PRODUCTS ----
        public static async Task<List<Product>> GetProductsAsync()
        {
            var rowsTask = GetTable<ProductDto>("products", "order=name.asc");
            var catsTask = GetTable<CategoryDto>("categories", "order=sort_order.asc");
            var supsTask = GetTable<SupplierDto>("suppliers", "order=company_name.asc");
            await Task.WhenAll(rowsTask, catsTask, supsTask);
            var rows = rowsTask.Result; var cats = catsTask.Result; var sups = supsTask.Result;
            return rows.Select(x => new Product
            {
                Id = x.id, Sku = x.sku, Barcode = x.barcode, Name = x.name,
                Description = x.description, CategoryId = x.category_id, SupplierId = x.supplier_id,
                PurchasePrice = x.purchase_price, SalePrice = x.sale_price, TaxRate = x.tax_rate,
                Color = x.color, Material = x.material, ImageUrl = x.image_url,
                MinStockLevel = x.min_stock_level, IsActive = x.is_active,
                CategoryName = cats.FirstOrDefault(c => c.id == x.category_id)?.name,
                SupplierName = sups.FirstOrDefault(s => s.id == x.supplier_id)?.company_name
            }).ToList();
        }

        public static async Task SaveProductAsync(Product p)
        {
            var data = new { sku = p.Sku, barcode = p.Barcode, name = p.Name, description = p.Description, category_id = p.CategoryId, supplier_id = p.SupplierId, purchase_price = p.PurchasePrice, sale_price = p.SalePrice, tax_rate = p.TaxRate, color = p.Color, material = p.Material, image_url = p.ImageUrl, min_stock_level = p.MinStockLevel, is_active = p.IsActive };
            if (string.IsNullOrEmpty(p.Id)) await Insert<object>("products", data);
            else await Update("products", p.Id, data);
        }

        public static async Task DeleteProductAsync(string id) => await Delete("products", id);

        // ---- STOCK ----
        public static async Task<List<StockItem>> GetStockAsync()
        {
            var rowsTask = GetTable<StockDto>("stock", "");
            var prodsTask = GetTable<ProductDto>("products", "order=name.asc");
            var whsTask = GetTable<WarehouseDto>("warehouses", "");
            await Task.WhenAll(rowsTask, prodsTask, whsTask);
            var rows = rowsTask.Result; var prods = prodsTask.Result; var whs = whsTask.Result;
            return rows.Select(x => new StockItem
            {
                Id = x.id, ProductId = x.product_id, WarehouseId = x.warehouse_id,
                Quantity = x.quantity, ReservedQuantity = x.reserved_quantity,
                ProductName = prods.FirstOrDefault(p => p.id == x.product_id)?.name,
                Sku = prods.FirstOrDefault(p => p.id == x.product_id)?.sku,
                WarehouseName = whs.FirstOrDefault(w => w.id == x.warehouse_id)?.name
            }).ToList();
        }

        // ---- STOCK MOVEMENTS ----
        public static async Task<List<StockMovement>> GetStockMovementsAsync()
        {
            var rowsTask = GetTable<StockMovementDto>("stock_movements", "order=created_at.desc");
            var prodsTask = GetTable<ProductDto>("products", "");
            var whsTask = GetTable<WarehouseDto>("warehouses", "");
            await Task.WhenAll(rowsTask, prodsTask, whsTask);
            var rows = rowsTask.Result; var prods = prodsTask.Result; var whs = whsTask.Result;
            return rows.Select(x => new StockMovement
            {
                Id = x.id, ProductId = x.product_id, WarehouseId = x.warehouse_id,
                MovementType = x.movement_type, Quantity = x.quantity,
                UnitPrice = x.unit_price, ReferenceNo = x.reference_no, Notes = x.notes,
                CreatedAt = DateTime.TryParse(x.created_at, out var d) ? d : DateTime.Now,
                ProductName = prods.FirstOrDefault(p => p.id == x.product_id)?.name,
                WarehouseName = whs.FirstOrDefault(w => w.id == x.warehouse_id)?.name
            }).ToList();
        }

        public static async Task AddStockMovementAsync(StockMovement m)
        {
            await Insert<object>("stock_movements", new { product_id = m.ProductId, warehouse_id = m.WarehouseId, movement_type = m.MovementType, quantity = m.Quantity, unit_price = m.UnitPrice, reference_no = m.ReferenceNo, notes = m.Notes, created_by = CurrentUser?.Id });
        }

        // ---- CUSTOMERS ----
        public static async Task<List<Customer>> GetCustomersAsync()
        {
            var rows = await GetTable<CustomerDto>("customers", "order=full_name.asc");
            return rows.Select(x => new Customer { Id = x.id, FullName = x.full_name, Email = x.email, Phone = x.phone, Address = x.address, City = x.city, TaxNumber = x.tax_number, Notes = x.notes, TotalOrders = x.total_orders, TotalSpent = x.total_spent, IsActive = x.is_active }).ToList();
        }

        public static async Task SaveCustomerAsync(Customer c)
        {
            var data = new { full_name = c.FullName, email = c.Email, phone = c.Phone, address = c.Address, city = c.City, tax_number = c.TaxNumber, notes = c.Notes, is_active = c.IsActive };
            if (string.IsNullOrEmpty(c.Id)) await Insert<object>("customers", data);
            else await Update("customers", c.Id, data);
        }

        // ---- ORDERS ----
        public static async Task<List<Order>> GetOrdersAsync()
        {
            var rowsTask = GetTable<OrderDto>("orders", "order=created_at.desc");
            var custsTask = GetTable<CustomerDto>("customers", "");
            await Task.WhenAll(rowsTask, custsTask);
            var rows = rowsTask.Result; var custs = custsTask.Result;
            return rows.Select(x => new Order
            {
                Id = x.id, OrderNo = x.order_no ?? "", CustomerId = x.customer_id,
                OrderStatus = x.order_status, PaymentStatus = x.payment_status,
                Subtotal = x.subtotal, TaxTotal = x.tax_total, DiscountTotal = x.discount_total,
                GrandTotal = x.grand_total, ShippingAddress = x.shipping_address, Notes = x.notes,
                CreatedAt = DateTime.TryParse(x.created_at, out var d) ? d : DateTime.Now,
                CustomerName = custs.FirstOrDefault(c => c.id == x.customer_id)?.full_name
            }).ToList();
        }

        public static async Task<string> CreateOrderAsync(Order o, List<OrderItem> items)
        {
            var result = await Insert<OrderDto>("orders", new { customer_id = o.CustomerId, subtotal = o.Subtotal, tax_total = o.TaxTotal, discount_total = o.DiscountTotal, grand_total = o.GrandTotal, shipping_address = o.ShippingAddress, notes = o.Notes, created_by = CurrentUser?.Id });
            var orderId = result?.id ?? "";
            foreach (var item in items)
            {
                await Insert<object>("order_items", new { order_id = orderId, product_id = item.ProductId, quantity = item.Quantity, unit_price = item.UnitPrice, tax_rate = item.TaxRate, discount_rate = item.DiscountRate, line_total = item.LineTotal });
            }
            return orderId;
        }

        // ---- EMPLOYEES ----
        public static async Task<List<Employee>> GetEmployeesAsync()
        {
            var rowsTask = GetTable<EmployeeDto>("employees", "");
            var profilesTask = GetTable<ProfileDto>("profiles", "");
            await Task.WhenAll(rowsTask, profilesTask);
            var rows = rowsTask.Result; var profiles = profilesTask.Result;
            return rows.Select(x =>
            {
                var p = profiles.FirstOrDefault(pr => pr.id == x.profile_id);
                return new Employee
                {
                    Id = x.id, ProfileId = x.profile_id, EmployeeNo = x.employee_no ?? "",
                    Department = x.department, Position = x.position,
                    HireDate = DateTime.TryParse(x.hire_date, out var h) ? h : DateTime.Now,
                    BirthDate = DateTime.TryParse(x.birth_date, out var b) ? b : null,
                    NationalId = x.national_id, Address = x.address, City = x.city,
                    EmergencyContact = x.emergency_contact, EmergencyPhone = x.emergency_phone,
                    Salary = x.salary, Status = x.status, Notes = x.notes,
                    FullName = p?.full_name, Email = p?.email, Phone = p?.phone
                };
            }).ToList();
        }

        public static async Task SaveEmployeeAsync(Employee e, string? password = null)
        {
            if (string.IsNullOrEmpty(e.Id))
            {
                var pr = await Insert<ProfileDto>("profiles", new { email = e.Email ?? $"emp{DateTime.Now.Ticks}@huzurmobilya.com", password_hash = HashPassword(password ?? "123456"), full_name = e.FullName ?? "", phone = e.Phone, role = "employee" });
                await Insert<object>("employees", new { profile_id = pr?.id, department = e.Department, position = e.Position, hire_date = e.HireDate.ToString("yyyy-MM-dd"), birth_date = e.BirthDate?.ToString("yyyy-MM-dd"), national_id = e.NationalId, address = e.Address, city = e.City, emergency_contact = e.EmergencyContact, emergency_phone = e.EmergencyPhone, salary = e.Salary, status = e.Status, notes = e.Notes });
            }
            else
            {
                await Update("employees", e.Id, new { department = e.Department, position = e.Position, hire_date = e.HireDate.ToString("yyyy-MM-dd"), birth_date = e.BirthDate?.ToString("yyyy-MM-dd"), national_id = e.NationalId, address = e.Address, city = e.City, emergency_contact = e.EmergencyContact, emergency_phone = e.EmergencyPhone, salary = e.Salary, status = e.Status, notes = e.Notes });
            }
        }

        // ---- LEAVE REQUESTS ----
        public static async Task<List<LeaveRequest>> GetLeaveRequestsAsync()
        {
            var rowsTask = GetTable<LeaveRequestDto>("leave_requests", "order=start_date.desc");
            var empsTask = GetTable<EmployeeDto>("employees", "");
            var profilesTask = GetTable<ProfileDto>("profiles", "");
            await Task.WhenAll(rowsTask, empsTask, profilesTask);
            var rows = rowsTask.Result; var emps = empsTask.Result; var profiles = profilesTask.Result;
            return rows.Select(x => new LeaveRequest
            {
                Id = x.id, EmployeeId = x.employee_id, LeaveType = x.leave_type,
                StartDate = DateTime.TryParse(x.start_date, out var s) ? s : DateTime.Now,
                EndDate = DateTime.TryParse(x.end_date, out var ed) ? ed : DateTime.Now,
                TotalDays = x.total_days, Reason = x.reason, Status = x.status,
                EmployeeName = profiles.FirstOrDefault(p => emps.Any(e => e.id == x.employee_id && e.profile_id == p.id))?.full_name
            }).ToList();
        }

        public static async Task SaveLeaveRequestAsync(LeaveRequest l)
        {
            var data = new { employee_id = l.EmployeeId, leave_type = l.LeaveType, start_date = l.StartDate.ToString("yyyy-MM-dd"), end_date = l.EndDate.ToString("yyyy-MM-dd"), total_days = l.TotalDays, reason = l.Reason, status = l.Status };
            if (string.IsNullOrEmpty(l.Id)) await Insert<object>("leave_requests", data);
            else await Update("leave_requests", l.Id, data);
        }

        // ---- NOTIFICATIONS ----
        public static async Task<List<Notification>> GetNotificationsAsync(string userId)
        {
            var rows = await GetTable<NotificationDto>("notifications", $"user_id=eq.{userId}&order=created_at.desc");
            return rows.Select(x => new Notification { Id = x.id, UserId = x.user_id, Title = x.title, Message = x.message, IsRead = x.is_read, CreatedAt = DateTime.TryParse(x.created_at, out var d) ? d : DateTime.Now }).ToList();
        }

        public static async Task MarkNotificationReadAsync(string id)
        {
            await Update("notifications", id, new { is_read = true });
        }

        // ---- IMAGE UPLOAD ----
        public static async Task<string> UploadImageAsync(string localFilePath)
        {
            var ext = Path.GetExtension(localFilePath).ToLower().TrimStart('.');
            var contentType = ext == "png" ? "image/png" : ext == "webp" ? "image/webp" : "image/jpeg";
            var fileName = $"products/{Guid.NewGuid():N}.{ext}";
            var bytes = await File.ReadAllBytesAsync(localFilePath);
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            var req = new HttpRequestMessage(HttpMethod.Post,
                $"{_url}/storage/v1/object/product-images/{fileName}");
            req.Headers.Add("apikey", _key);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _key);
            req.Content = content;

            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                throw new Exception("Resim yüklenemedi: " + await resp.Content.ReadAsStringAsync());

            return $"{_url}/storage/v1/object/public/product-images/{fileName}";
        }
    }

    // ---- DTOs ----
    public class ProfileDto { public string id { get; set; } = ""; public string email { get; set; } = ""; public string password_hash { get; set; } = ""; public string full_name { get; set; } = ""; public string? phone { get; set; } public string role { get; set; } = "employee"; public bool is_active { get; set; } = true; }
    public class CategoryDto { public string id { get; set; } = ""; public string name { get; set; } = ""; public string? description { get; set; } public string? parent_id { get; set; } public int sort_order { get; set; } public bool is_active { get; set; } = true; }
    public class SupplierDto { public string id { get; set; } = ""; public string company_name { get; set; } = ""; public string? contact_name { get; set; } public string? email { get; set; } public string? phone { get; set; } public string? address { get; set; } public string? city { get; set; } public string? tax_number { get; set; } public string? tax_office { get; set; } public string? notes { get; set; } public bool is_active { get; set; } = true; }
    public class WarehouseDto { public string id { get; set; } = ""; public string name { get; set; } = ""; public string? address { get; set; } public string? city { get; set; } public int capacity { get; set; } public bool is_active { get; set; } = true; }
    public class ProductDto { public string id { get; set; } = ""; public string sku { get; set; } = ""; public string? barcode { get; set; } public string name { get; set; } = ""; public string? description { get; set; } public string? category_id { get; set; } public string? supplier_id { get; set; } public decimal purchase_price { get; set; } public decimal sale_price { get; set; } public decimal tax_rate { get; set; } = 18; public string? color { get; set; } public string? material { get; set; } public string? image_url { get; set; } public int min_stock_level { get; set; } = 5; public bool is_active { get; set; } = true; }
    public class StockDto { public string id { get; set; } = ""; public string product_id { get; set; } = ""; public string warehouse_id { get; set; } = ""; public int quantity { get; set; } public int reserved_quantity { get; set; } }
    public class StockMovementDto { public string id { get; set; } = ""; public string product_id { get; set; } = ""; public string warehouse_id { get; set; } = ""; public string movement_type { get; set; } = ""; public int quantity { get; set; } public decimal? unit_price { get; set; } public string? reference_no { get; set; } public string? notes { get; set; } public string? created_by { get; set; } public string? created_at { get; set; } }
    public class CustomerDto { public string id { get; set; } = ""; public string full_name { get; set; } = ""; public string? email { get; set; } public string? phone { get; set; } public string? address { get; set; } public string? city { get; set; } public string? tax_number { get; set; } public string? notes { get; set; } public int total_orders { get; set; } public decimal total_spent { get; set; } public bool is_active { get; set; } = true; }
    public class OrderDto { public string id { get; set; } = ""; public string? order_no { get; set; } public string? customer_id { get; set; } public string order_status { get; set; } = "beklemede"; public string payment_status { get; set; } = "odenmedi"; public decimal subtotal { get; set; } public decimal tax_total { get; set; } public decimal discount_total { get; set; } public decimal grand_total { get; set; } public string? shipping_address { get; set; } public string? notes { get; set; } public string? created_by { get; set; } public string? created_at { get; set; } }
    public class EmployeeDto { public string id { get; set; } = ""; public string profile_id { get; set; } = ""; public string? employee_no { get; set; } public string department { get; set; } = ""; public string position { get; set; } = ""; public string hire_date { get; set; } = ""; public string? birth_date { get; set; } public string? national_id { get; set; } public string? address { get; set; } public string? city { get; set; } public string? emergency_contact { get; set; } public string? emergency_phone { get; set; } public decimal? salary { get; set; } public string status { get; set; } = "aktif"; public string? notes { get; set; } }
    public class LeaveRequestDto { public string id { get; set; } = ""; public string employee_id { get; set; } = ""; public string leave_type { get; set; } = ""; public string start_date { get; set; } = ""; public string end_date { get; set; } = ""; public int total_days { get; set; } public string? reason { get; set; } public string status { get; set; } = "beklemede"; }
    public class NotificationDto { public string id { get; set; } = ""; public string user_id { get; set; } = ""; public string title { get; set; } = ""; public string message { get; set; } = ""; public bool is_read { get; set; } public string? created_at { get; set; } }
}
