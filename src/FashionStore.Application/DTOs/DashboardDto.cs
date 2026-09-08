namespace FashionStore.Application.DTOs;

/// <summary>آمار داشبورد مدیریت</summary>
public class DashboardStatsDto
{
    public int TotalOrders { get; set; }
    public int TodayOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockProducts { get; set; }
    public List<RevenueChartDto> WeeklyRevenue { get; set; } = new();
    public List<OrderStatusChartDto> OrderStatusChart { get; set; } = new();
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
}

public class RevenueChartDto
{
    public string Date { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class OrderStatusChartDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
