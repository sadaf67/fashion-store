using FashionStore.Application.Interfaces;
using FashionStore.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.WebUI.Controllers;

/// <summary>فروشگاه - لیست محصولات و جزئیات</summary>
public class ShopController : Controller
{
    private readonly IProductRepository _products;
    private readonly ApplicationDbContext _db;

    public ShopController(IProductRepository products, ApplicationDbContext db)
    {
        _products = products;
        _db = db;
    }

    /// <summary>لیست محصولات با قابلیت فیلتر و جستجو</summary>
    public async Task<IActionResult> Index(int? categoryId, string? search, string? sort)
    {
        var categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
        ViewBag.Categories = categories;
        ViewBag.SelectedCategory = categoryId;
        ViewBag.Search = search;

        IEnumerable<Domain.Entities.Product> products;

        if (!string.IsNullOrEmpty(search))
            products = await _products.SearchAsync(search);
        else if (categoryId.HasValue)
            products = await _products.GetByCategoryAsync(categoryId.Value);
        else
            products = await _products.GetAllAsync();

        // مرتب‌سازی
        products = sort switch
        {
            "price_asc" => products.OrderBy(p => p.DiscountedPrice ?? p.Price),
            "price_desc" => products.OrderByDescending(p => p.DiscountedPrice ?? p.Price),
            "newest" => products.OrderByDescending(p => p.CreatedAt),
            _ => products.OrderByDescending(p => p.IsFeatured)
        };

        return View(products);
    }

    /// <summary>صفحه جزئیات محصول</summary>
    public async Task<IActionResult> Detail(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product == null) return NotFound();

        // محصولات مشابه از همین دسته‌بندی
        var related = await _products.GetByCategoryAsync(product.CategoryId);
        ViewBag.RelatedProducts = related.Where(p => p.Id != id).Take(4);

        return View(product);
    }
}
