using FashionStore.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.WebUI.Areas.Admin.Controllers;

/// <summary>داشبورد مدیریت - آمار و نمودار</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
        => _dashboardService = dashboardService;

    /// <summary>صفحه اصلی داشبورد با آمار کامل</summary>
    public async Task<IActionResult> Index()
    {
        var stats = await _dashboardService.GetStatsAsync();
        return View(stats);
    }
}
