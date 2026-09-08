using FashionStore.Domain.Common;
using FashionStore.Domain.Enums;

namespace FashionStore.Domain.Entities;

/// <summary>کد تخفیف</summary>
public class Discount : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType Type { get; set; }
    public decimal Value { get; set; }          // درصد یا مبلغ
    public decimal? MinOrderAmount { get; set; } // حداقل مبلغ سفارش
    public decimal? MaxDiscountAmount { get; set; } // سقف تخفیف
    public int? UsageLimit { get; set; }        // محدودیت تعداد استفاده
    public int UsedCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CategoryId { get; set; }        // اگر null باشه، برای همه کالاها

    public Category? Category { get; set; }
}
