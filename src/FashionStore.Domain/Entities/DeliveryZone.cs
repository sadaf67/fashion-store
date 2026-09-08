using FashionStore.Domain.Common;

namespace FashionStore.Domain.Entities;

/// <summary>محدوده پیک - هر زون یک ناحیه جغرافیایی است</summary>
public class DeliveryZone : BaseEntity
{
    public string Name { get; set; } = string.Empty;     // مثلاً: تهران - منطقه 1
    public string? Description { get; set; }
    public decimal DeliveryFee { get; set; }             // هزینه ارسال
    public int EstimatedDays { get; set; }               // زمان تخمینی تحویل
    public bool IsActive { get; set; } = true;
    public string? CoverageArea { get; set; }            // توضیح محدوده (متن)

    // محدوده روی نقشه به صورت polygon JSON
    public string? MapPolygonJson { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
