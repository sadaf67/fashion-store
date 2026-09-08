using FashionStore.Domain.Entities;

namespace FashionStore.Application.Interfaces;

/// <summary>سرویس کد تخفیف</summary>
public interface IDiscountService
{
    Task<Discount?> ValidateCodeAsync(string code, decimal orderAmount);
    Task<decimal> CalculateDiscountAsync(Discount discount, decimal orderAmount);
    Task IncrementUsageAsync(int discountId);
    Task<bool> IsValidAsync(string code);
}
