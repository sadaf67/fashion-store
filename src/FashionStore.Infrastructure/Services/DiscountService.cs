using FashionStore.Application.Interfaces;
using FashionStore.Domain.Entities;
using FashionStore.Domain.Enums;
using FashionStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.Infrastructure.Services;

/// <summary>سرویس اعتبارسنجی و محاسبه کد تخفیف</summary>
public class DiscountService : IDiscountService
{
    private readonly ApplicationDbContext _db;

    public DiscountService(ApplicationDbContext db) => _db = db;

    public async Task<Discount?> ValidateCodeAsync(string code, decimal orderAmount)
    {
        var now = DateTime.UtcNow;
        var discount = await _db.Discounts
            .FirstOrDefaultAsync(d =>
                d.Code == code &&
                d.IsActive &&
                !d.IsDeleted &&
                d.StartDate <= now &&
                d.EndDate >= now);

        if (discount == null) return null;

        // بررسی محدودیت استفاده
        if (discount.UsageLimit.HasValue && discount.UsedCount >= discount.UsageLimit.Value)
            return null;

        // بررسی حداقل مبلغ سفارش
        if (discount.MinOrderAmount.HasValue && orderAmount < discount.MinOrderAmount.Value)
            return null;

        return discount;
    }

    public Task<decimal> CalculateDiscountAsync(Discount discount, decimal orderAmount)
    {
        decimal discountAmount = discount.Type == DiscountType.Percentage
            ? orderAmount * (discount.Value / 100)
            : discount.Value;

        // اعمال سقف تخفیف
        if (discount.MaxDiscountAmount.HasValue)
            discountAmount = Math.Min(discountAmount, discount.MaxDiscountAmount.Value);

        return Task.FromResult(Math.Min(discountAmount, orderAmount));
    }

    public async Task IncrementUsageAsync(int discountId)
    {
        var discount = await _db.Discounts.FindAsync(discountId);
        if (discount != null)
        {
            discount.UsedCount++;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsValidAsync(string code)
    {
        var result = await ValidateCodeAsync(code, 0);
        return result != null;
    }
}
