using FashionStore.Application.Interfaces;
using FashionStore.Domain.Entities;
using FashionStore.Domain.Enums;
using FashionStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.WebUI.Areas.Admin.Controllers;

/// <summary>پنل ارسال پیامک - انبوه و فردی</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SmsController : Controller
{
    private readonly ISmsService _sms;
    private readonly ApplicationDbContext _db;

    public SmsController(ISmsService sms, ApplicationDbContext db)
    {
        _sms = sms;
        _db = db;
    }

    /// <summary>لاگ پیامک‌های ارسال شده + فرم ارسال</summary>
    public async Task<IActionResult> Index()
    {
        var logs = await _db.SmsLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(100)
            .ToListAsync();
        var templates = await _db.SmsTemplates.Where(t => t.IsActive).ToListAsync();
        ViewBag.Templates = templates;
        return View(logs);
    }

    /// <summary>ارسال پیامک به یک یا چند شماره</summary>
    [HttpPost]
    public async Task<IActionResult> Send(string phones, string message)
    {
        var phoneList = phones.Split(new[] { '\n', ',', '،' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(p => p.Trim())
                              .Where(p => !string.IsNullOrEmpty(p))
                              .ToList();

        if (!phoneList.Any())
        {
            TempData["Error"] = "شماره‌ای وارد نشده";
            return RedirectToAction(nameof(Index));
        }

        var success = await _sms.SendBulkAsync(phoneList, message);
        TempData["Success"] = success
            ? $"پیامک به {phoneList.Count} نفر ارسال شد"
            : "خطا در ارسال پیامک";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>ارسال پیامک به تمام کاربران</summary>
    [HttpPost]
    public async Task<IActionResult> SendToAll(string message)
    {
        var phones = await _db.Users
            .Where(u => u.PhoneNumber != null && u.IsActive)
            .Select(u => u.PhoneNumber!)
            .ToListAsync();

        await _sms.SendBulkAsync(phones, message);
        TempData["Success"] = $"پیامک به {phones.Count} کاربر ارسال شد";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>ذخیره قالب پیامک</summary>
    [HttpPost]
    public async Task<IActionResult> SaveTemplate(SmsTemplate smsTemplate)
    {
        if (smsTemplate.Id > 0)
            _db.SmsTemplates.Update(smsTemplate);
        else
            _db.SmsTemplates.Add(smsTemplate);

        await _db.SaveChangesAsync();
        TempData["Success"] = "قالب ذخیره شد";
        return RedirectToAction(nameof(Index));
    }
}
