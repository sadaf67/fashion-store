using FashionStore.Domain.Common;
using FashionStore.Domain.Enums;

namespace FashionStore.Domain.Entities;

/// <summary>سفارش</summary>
public class Order : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal FinalAmount { get; set; }
    public string? DiscountCode { get; set; }
    public int? DeliveryZoneId { get; set; }
    public int AddressId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public ApplicationUser? User { get; set; }
    public Address? Address { get; set; }
    public DeliveryZone? DeliveryZone { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
