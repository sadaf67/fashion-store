using FashionStore.Application.DTOs;
using FashionStore.Domain.Enums;
using FashionStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.Infrastructure.Services;

/// <summary>سرویس آمار داشبورد مدیریت</summary>
public class DashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db) => _db = db;

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var weekAgo = today.AddDays(-6);

        // آمار کلی
        var totalOrders = await _db.Orders.CountAsync();
        var todayOrders = await _db.Orders.CountAsync(o => o.CreatedAt.Date == today);
        var totalRevenue = await _db.Orders.Where(o => o.Status != OrderStatus.Cancelled).SumAsync(o => o.FinalAmount);
        var monthRevenue = await _db.Orders.Where(o => o.Status != OrderStatus.Cancelled && o.CreatedAt >= monthStart).SumAsync(o => o.FinalAmount);
        var totalProducts = await _db.Products.CountAsync();
        var totalUsers = await _db.Users.CountAsync();
        var pendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
        var lowStock = await _db.ProductVariants.CountAsync(v => v.StockQuantity < 5);

        // نمودار درآمد هفتگی
        var weeklyRevenue = new List<RevenueChartDto>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var amount = await _db.Orders
                .Where(o => o.CreatedAt.Date == date && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.FinalAmount);
            weeklyRevenue.Add(new RevenueChartDto { Date = date.ToString("MM/dd"), Amount = amount });
        }

        // نمودار وضعیت سفارشات
        var statusChart = await _db.Orders
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusChartDto { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        // سفارشات اخیر
        var recentOrders = await _db.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .Select(o => new RecentOrderDto
            {
                OrderNumber = o.OrderNumber,
                CustomerName = o.User!.FullName,
                FinalAmount = o.FinalAmount,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalOrders = totalOrders,
            TodayOrders = todayOrders,
            TotalRevenue = totalRevenue,
            MonthRevenue = monthRevenue,
            TotalProducts = totalProducts,
            TotalUsers = totalUsers,
            PendingOrders = pendingOrders,
            LowStockProducts = lowStock,
            WeeklyRevenue = weeklyRevenue,
            OrderStatusChart = statusChart,
            RecentOrders = recentOrders
        };
    }
}
