using FashionStore.Domain.Entities;
using FashionStore.Domain.Enums;

namespace FashionStore.Application.Interfaces;

/// <summary>قرارداد repository سفارشات</summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
    Task<IEnumerable<Order>> GetAllAsync(OrderStatus? status = null);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<int> GetTotalCountAsync();
    Task<decimal> GetTotalRevenueAsync(DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);
}
