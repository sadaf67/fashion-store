using FashionStore.Application.Interfaces;
using FashionStore.Domain.Entities;
using FashionStore.Domain.Enums;
using FashionStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.WebUI.Areas.Admin.Controllers;

/// <summary>مدیریت محصولات - CRUD کامل با آپلود تصویر</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly IProductRepository _products;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(IProductRepository products, ApplicationDbContext db, IWebHostEnvironment env)
    {
        _products = products;
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _products.GetAllAsync(includeInactive: true);
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product model, IFormFile? primaryImage, List<IFormFile>? additionalImages, IFormFile? tryOnImage)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        await _products.AddAsync(model);

        // آپلود تصویر اصلی
        if (primaryImage != null)
        {
            var url = await SaveImageAsync(primaryImage, "products");
            _db.ProductImages.Add(new ProductImage { ProductId = model.Id, ImageUrl = url, IsPrimary = true, SortOrder = 0 });
        }

        // آپلود تصاویر اضافی
        if (additionalImages != null)
            for (int i = 0; i < additionalImages.Count; i++)
            {
                var url = await SaveImageAsync(additionalImages[i], "products");
                _db.ProductImages.Add(new ProductImage { ProductId = model.Id, ImageUrl = url, IsPrimary = false, SortOrder = i + 1 });
            }

        // تصویر overlay برای پرو آنلاین
        if (tryOnImage != null)
        {
            var url = await SaveImageAsync(tryOnImage, "tryon");
            model.TryOnOverlayImageUrl = url;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "محصول با موفقیت اضافه شد";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product == null) return NotFound();
        ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Product model, IFormFile? primaryImage, IFormFile? tryOnImage)
    {
        var existing = await _products.GetByIdAsync(model.Id);
        if (existing == null) return NotFound();

        // بروزرسانی فیلدها
        existing.Name = model.Name;
        existing.Description = model.Description;
        existing.ShortDescription = model.ShortDescription;
        existing.Price = model.Price;
        existing.DiscountedPrice = model.DiscountedPrice;
        existing.CategoryId = model.CategoryId;
        existing.Brand = model.Brand;
        existing.Status = model.Status;
        existing.IsFeatured = model.IsFeatured;
        existing.IsTryOnEnabled = model.IsTryOnEnabled;
        existing.Tags = model.Tags;

        if (primaryImage != null)
        {
            var url = await SaveImageAsync(primaryImage, "products");
            var oldPrimary = existing.Images.FirstOrDefault(i => i.IsPrimary);
            if (oldPrimary != null) oldPrimary.ImageUrl = url;
            else _db.ProductImages.Add(new ProductImage { ProductId = model.Id, ImageUrl = url, IsPrimary = true });
        }

        if (tryOnImage != null)
            existing.TryOnOverlayImageUrl = await SaveImageAsync(tryOnImage, "tryon");

        await _products.UpdateAsync(existing);
        TempData["Success"] = "محصول بروزرسانی شد";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _products.DeleteAsync(id);
        TempData["Success"] = "محصول حذف شد";
        return RedirectToAction(nameof(Index));
    }

    // ─── API: مدیریت واریانت‌ها ───────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> AddVariant(int productId, string size, string color, string? colorHex, int stock, decimal? priceAdj)
    {
        _db.ProductVariants.Add(new ProductVariant
        {
            ProductId = productId, Size = size, Color = color,
            ColorHex = colorHex, StockQuantity = stock, PriceAdjustment = priceAdj
        });
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }

    // ─── ذخیره تصویر در سرور ─────────────────────────────────────────────────
    private async Task<string> SaveImageAsync(IFormFile file, string folder)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };
        if (file.Length <= 0 || file.Length > 5 * 1024 * 1024 || !allowedExtensions.Contains(extension) || !allowedContentTypes.Contains(file.ContentType))
            throw new InvalidDataException("فایل تصویر معتبر نیست یا بیش از ۵ مگابایت حجم دارد.");
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(uploadsDir, fileName);
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{folder}/{fileName}";
    }
}
