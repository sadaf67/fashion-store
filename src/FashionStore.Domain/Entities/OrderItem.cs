using FashionStore.Domain.Common;

namespace FashionStore.Domain.Entities;

/// <summary>آیتم سفارش</summary>
public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty; // ذخیره نام در لحظه خرید
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
