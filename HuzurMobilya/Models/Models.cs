using System;

namespace HuzurMobilya.Models
{
    public class Profile
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public string Role { get; set; } = "employee";
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Category
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Supplier
    {
        public string Id { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? TaxNumber { get; set; }
        public string? TaxOffice { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Warehouse
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Address { get; set; }
        public string? City { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Product
    {
        public string Id { get; set; } = "";
        public string Sku { get; set; } = "";
        public string? Barcode { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? CategoryId { get; set; }
        public string? SupplierId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TaxRate { get; set; } = 18;
        public string? Color { get; set; }
        public string? Material { get; set; }
        public string? ImageUrl { get; set; }
        public int MinStockLevel { get; set; } = 5;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Display helpers
        public string? CategoryName { get; set; }
        public string? SupplierName { get; set; }
        public int TotalStock { get; set; }
    }

    public class StockItem
    {
        public string Id { get; set; } = "";
        public string ProductId { get; set; } = "";
        public string WarehouseId { get; set; } = "";
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }

        public string? ProductName { get; set; }
        public string? WarehouseName { get; set; }
        public string? Sku { get; set; }
    }

    public class StockMovement
    {
        public string Id { get; set; } = "";
        public string ProductId { get; set; } = "";
        public string WarehouseId { get; set; } = "";
        public string MovementType { get; set; } = "giris";
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ProductName { get; set; }
        public string? WarehouseName { get; set; }
    }

    public class Customer
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? TaxNumber { get; set; }
        public string? Notes { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Order
    {
        public string Id { get; set; } = "";
        public string OrderNo { get; set; } = "";
        public string? CustomerId { get; set; }
        public string OrderStatus { get; set; } = "beklemede";
        public string PaymentStatus { get; set; } = "odenmedi";
        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? CustomerName { get; set; }
    }

    public class OrderItem
    {
        public string Id { get; set; } = "";
        public string OrderId { get; set; } = "";
        public string ProductId { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; } = 18;
        public decimal DiscountRate { get; set; }
        public decimal LineTotal { get; set; }

        public string? ProductName { get; set; }
        public string? Sku { get; set; }
    }

    public class Employee
    {
        public string Id { get; set; } = "";
        public string ProfileId { get; set; } = "";
        public string EmployeeNo { get; set; } = "";
        public string Department { get; set; } = "";
        public string Position { get; set; } = "";
        public DateTime HireDate { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? NationalId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public decimal? Salary { get; set; }
        public string Status { get; set; } = "aktif";
        public string? Notes { get; set; }

        // Display
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class LeaveRequest
    {
        public string Id { get; set; } = "";
        public string EmployeeId { get; set; } = "";
        public string LeaveType { get; set; } = "yillik";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "beklemede";
        public string? EmployeeName { get; set; }
    }

    public class DashboardStats
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int ActiveEmployees { get; set; }
        public int MonthlyOrders { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingTasks { get; set; }
    }

    public class Notification
    {
        public string Id { get; set; } = "";
        public string UserId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
