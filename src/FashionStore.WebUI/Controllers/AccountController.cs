using FashionStore.Application.Interfaces;
using FashionStore.Domain.Entities;
using FashionStore.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FashionStore.WebUI.Controllers;

/// <summary>احراز هویت کاربر - ثبت نام، ورود، OTP</summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ISmsService _sms;
    private readonly ApplicationDbContext _db;
    private readonly IOrderRepository _orders;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ISmsService sms, ApplicationDbContext db, IOrderRepository orders)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _sms = sms;
        _db = db;
        _orders = orders;
    }

    // ─── ثبت نام ──────────────────────────────────────────────────────────────
    [HttpGet] public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.Phone,
            RegisteredAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Customer");
            await _signInManager.SignInAsync(user, isPersistent: false);

            // ارسال پیامک خوشامدگویی
            if (!string.IsNullOrEmpty(model.Phone))
                await _sms.SendAsync(model.Phone, $"خوش آمدید {model.FullName}! به فروشگاه مد الگانت خوش آمدید.", Domain.Enums.SmsType.OtpLogin, user.Id);

            return RedirectToAction("Index", "Home");
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError("", err.Description);

        return View(model);
    }

    // ─── ورود ─────────────────────────────────────────────────────────────────
    [HttpGet] public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? "/");

        if (result.IsLockedOut)
            ModelState.AddModelError("", "حساب شما به دلیل تلاش‌های مکرر قفل شده است.");
        else
            ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است.");

        return View(model);
    }

    // ─── خروج ─────────────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ─── پروفایل کاربر ────────────────────────────────────────────────────────
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        var orders = await _orders.GetUserOrdersAsync(user!.Id);
        var addresses = await _db.Addresses.Where(a => a.UserId == user.Id && !a.IsDeleted).ToListAsync();
        ViewBag.Orders = orders;
        ViewBag.Addresses = addresses;
        return View(user);
    }

    public IActionResult AccessDenied() => View();

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(Address model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (string.IsNullOrWhiteSpace(model.ReceiverName) || string.IsNullOrWhiteSpace(model.Phone) ||
            string.IsNullOrWhiteSpace(model.City) || string.IsNullOrWhiteSpace(model.Street))
        {
            TempData["Error"] = "اطلاعات ضروری آدرس را کامل کنید";
            return RedirectToAction(nameof(Profile));
        }

        model.Id = 0;
        model.UserId = userId;
        model.CreatedAt = DateTime.UtcNow;
        if (model.IsDefault)
        {
            var currentDefaults = await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            foreach (var address in currentDefaults) address.IsDefault = false;
        }
        _db.Addresses.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "آدرس با موفقیت ذخیره شد";
        return RedirectToAction(nameof(Profile));
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address != null)
        {
            address.IsDeleted = true;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Profile));
    }
}

// ─── ViewModels ───────────────────────────────────────────────────────────────
public class RegisterViewModel
{
    [Required(ErrorMessage = "نام الزامی است")]
    [Display(Name = "نام و نام خانوادگی")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [Phone]
    [Display(Name = "شماره موبایل")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [MinLength(6, ErrorMessage = "رمز عبور حداقل ۶ کاراکتر")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "رمزها مطابقت ندارند")]
    [DataType(DataType.Password)]
    [Display(Name = "تکرار رمز عبور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "مرا به خاطر بسپار")]
    public bool RememberMe { get; set; }
}
