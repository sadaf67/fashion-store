using FashionStore.Domain.Entities;
using FashionStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.WebUI.Areas.Admin.Controllers;

/// <summary>تنظیمات سایت - همه چیز توسط مدیر</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public SettingsController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _db.SiteSettings.FirstOrDefaultAsync() ?? new SiteSettings();
        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> Save(SiteSettings model, IFormFile? logoFile, IFormFile? heroImageFile)
    {
        var settings = await _db.SiteSettings.FirstOrDefaultAsync();
        if (settings == null) { settings = new SiteSettings(); _db.SiteSettings.Add(settings); }

        settings.SiteName = model.SiteName;
        settings.HeroTitle = model.HeroTitle;
        settings.HeroSubtitle = model.HeroSubtitle;
        settings.ContactEmail = model.ContactEmail;
        settings.ContactPhone = model.ContactPhone;
        settings.Address = model.Address;
        settings.InstagramUrl = model.InstagramUrl;
        settings.TelegramUrl = model.TelegramUrl;
        settings.AboutText = model.AboutText;
        settings.SmsApiKey = model.SmsApiKey;
        settings.SmsSender = model.SmsSender;
        settings.IsMaintenanceMode = model.IsMaintenanceMode;

        // آپلود لوگو
        if (logoFile != null)
            settings.Logo = await SaveFileAsync(logoFile, "site");

        // آپلود تصویر hero
        if (heroImageFile != null)
            settings.HeroImageUrl = await SaveFileAsync(heroImageFile, "site");

        await _db.SaveChangesAsync();
        TempData["Success"] = "تنظیمات ذخیره شد";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };
        if (file.Length <= 0 || file.Length > 5 * 1024 * 1024 || !allowedExtensions.Contains(extension) || !allowedContentTypes.Contains(file.ContentType))
            throw new InvalidDataException("فایل تصویر معتبر نیست یا بیش از ۵ مگابایت حجم دارد.");
        var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{folder}/{fileName}";
    }
}
