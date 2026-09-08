using FashionStore.Domain.Common;

namespace FashionStore.Domain.Entities;

/// <summary>آدرس تحویل کاربر</summary>
public class Address : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public int? DeliveryZoneId { get; set; }

    public ApplicationUser? User { get; set; }
    public DeliveryZone? DeliveryZone { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
