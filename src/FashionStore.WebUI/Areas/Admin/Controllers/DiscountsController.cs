using FashionStore.Domain.Entities;
using FashionStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.WebUI.Areas.Admin.Controllers;

/// <summary>مدیریت کدهای تخفیف</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DiscountsController : Controller
{
    private readonly ApplicationDbContext _db;

    public DiscountsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.Discounts.Include(d => d.Category).OrderByDescending(d => d.CreatedAt).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Discount model)
    {
        // بررسی تکراری نبودن کد
        if (await _db.Discounts.AnyAsync(d => d.Code == model.Code))
        {
            ModelState.AddModelError("Code", "این کد قبلاً ثبت شده است");
            ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        _db.Discounts.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "کد تخفیف ایجاد شد";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var discount = await _db.Discounts.FindAsync(id);
        if (discount != null)
        {
            discount.IsActive = !discount.IsActive;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var discount = await _db.Discounts.FindAsync(id);
        if (discount != null) { discount.IsDeleted = true; await _db.SaveChangesAsync(); }
        TempData["Success"] = "کد تخفیف حذف شد";
        return RedirectToAction(nameof(Index));
    }
}
