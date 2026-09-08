using System.Diagnostics;
using FashionStore.Application.Interfaces;
using FashionStore.Infrastructure.Data;
using FashionStore.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.WebUI.Controllers;

/// <summary>صفحه اصلی سایت فروشگاه</summary>
public class HomeController : Controller
{
    private readonly IProductRepository _products;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IProductRepository products, ApplicationDbContext db, ILogger<HomeController> logger)
    {
        _products = products;
        _db = db;
        _logger = logger;
    }

    /// <summary>صفحه اصلی - نمایش محصولات ویژه و Hero داینامیک</summary>
    public async Task<IActionResult> Index()
    {
        var settings = await _db.SiteSettings.FirstOrDefaultAsync();
        var featured = await _products.GetFeaturedAsync(8);
        var categories = await _db.Categories
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        ViewBag.Settings = settings;
        ViewBag.Categories = categories;
        return View(featured);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
