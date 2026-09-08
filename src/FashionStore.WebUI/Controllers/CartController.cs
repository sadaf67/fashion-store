using FashionStore.Application.Interfaces;
using FashionStore.Domain.Entities;
using FashionStore.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FashionStore.WebUI.Controllers;

/// <summary>سبد خرید - افزودن، حذف، نمایش و تسویه</summary>
public class CartController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IDiscountService _discountService;
    private readonly IDeliveryZoneService _deliveryService;
    private readonly IOrderRepository _orders;
    private readonly ISmsService _sms;

    public CartController(ApplicationDbContext db, IDiscountService discountService,
        IDeliveryZoneService deliveryService, IOrderRepository orders, ISmsService sms)
    {
        _db = db;
        _discountService = discountService;
        _deliveryService = deliveryService;
        _orders = orders;
        _sms = sms;
    }

    /// <summary>نمایش سبد خرید</summary>
    public async Task<IActionResult> Index()
    {
        var cart = await GetOrCreateCartAsync();
        ViewBag.DeliveryZones = await _deliveryService.GetAllZonesAsync();
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Addresses = await _db.Addresses
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .ToListAsync();
        }
        return View(cart);
    }

    /// <summary>افزودن محصول به سبد (AJAX)</summary>
    [HttpPost]
    public async Task<IActionResult> Add(int productId, int? variantId, int quantity = 1)
    {
        quantity = Math.Clamp(quantity, 1, 20);
        var product = await _db.Products.Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == productId && p.Status == Domain.Enums.ProductStatus.Active);
        if (product == null)
            return BadRequest(new { success = false, message = "محصول در دسترس نیست" });

        ProductVariant? variant = null;
        if (variantId.HasValue)
        {
            variant = product.Variants.FirstOrDefault(v => v.Id == variantId && v.ProductId == productId);
            if (variant == null || variant.StockQuantity < quantity)
                return BadRequest(new { success = false, message = "تنوع انتخابی موجود نیست" });
        }

        var cart = await GetOrCreateCartAsync();

        // بررسی اگر محصول قبلاً وجود دارد
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == variantId);
        if (existing != null)
        {
            var newQuantity = existing.Quantity + quantity;
            if (variant != null && newQuantity > variant.StockQuantity)
                return BadRequest(new { success = false, message = "موجودی کافی نیست" });
            existing.Quantity = newQuantity;
        }
        else
            cart.Items.Add(new CartItem { CartId = cart.Id, ProductId = productId, ProductVariantId = variantId, Quantity = quantity });

        await _db.SaveChangesAsync();
        var count = cart.Items.Sum(i => i.Quantity);
        return Json(new { success = true, cartCount = count, message = "محصول به سبد اضافه شد" });
    }

    /// <summary>حذف آیتم از سبد (AJAX)</summary>
    [HttpPost]
    public async Task<IActionResult> Remove(int itemId)
    {
        var cart = await GetOrCreateCartAsync();
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null) { _db.CartItems.Remove(item); await _db.SaveChangesAsync(); }
        return Json(new { success = true });
    }

    /// <summary>بررسی کد تخفیف (AJAX)</summary>
    [HttpPost]
    public async Task<IActionResult> ApplyDiscount(string code, decimal orderAmount)
    {
        var discount = await _discountService.ValidateCodeAsync(code, orderAmount);
        if (discount == null)
            return Json(new { success = false, message = "کد تخفیف معتبر نیست یا منقضی شده" });

        var discountAmount = await _discountService.CalculateDiscountAsync(discount, orderAmount);
        return Json(new { success = true, discountAmount, discountId = discount.Id, message = $"تخفیف {discountAmount:N0} تومانی اعمال شد" });
    }

    /// <summary>ثبت سفارش نهایی</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Checkout(int addressId, int? discountId, int deliveryZoneId)
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToAction("Login", "Account", new { returnUrl = "/Cart" });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cart = await GetOrCreateCartAsync();

        if (!cart.Items.Any())
            return RedirectToAction("Index");

        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);
        var zone = await _deliveryService.GetZoneByIdAsync(deliveryZoneId);
        if (address == null || zone == null || !zone.IsActive)
        {
            TempData["Error"] = "آدرس یا روش ارسال معتبر نیست";
            return RedirectToAction("Index");
        }

        // محاسبه قیمت‌ها
        decimal total = 0;
        var items = new List<OrderItem>();
        foreach (var ci in cart.Items)
        {
            var product = await _db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == ci.ProductId);
            if (product == null) continue;
            var variant = ci.ProductVariantId.HasValue ? product.Variants.FirstOrDefault(v => v.Id == ci.ProductVariantId) : null;
            if (ci.ProductVariantId.HasValue && (variant == null || variant.StockQuantity < ci.Quantity))
            {
                TempData["Error"] = $"موجودی {product.Name} کافی نیست";
                return RedirectToAction("Index");
            }
            var unitPrice = (product.DiscountedPrice ?? product.Price) + (variant?.PriceAdjustment ?? 0);
            total += unitPrice * ci.Quantity;
            items.Add(new OrderItem
            {
                ProductId = ci.ProductId,
                ProductVariantId = ci.ProductVariantId,
                ProductName = product.Name,
                Size = variant?.Size,
                Color = variant?.Color,
                Quantity = ci.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice * ci.Quantity
            });
            if (variant != null) variant.StockQuantity -= ci.Quantity;
        }

        // اعمال تخفیف
        decimal discountAmount = 0;
        string? discountCode = null;
        if (discountId.HasValue)
        {
            var discount = await _db.Discounts.FindAsync(discountId.Value);
            if (discount != null)
            {
                var validDiscount = await _discountService.ValidateCodeAsync(discount.Code, total);
                if (validDiscount != null)
                {
                    discountAmount = await _discountService.CalculateDiscountAsync(validDiscount, total);
                    discountCode = validDiscount.Code;
                    await _discountService.IncrementUsageAsync(validDiscount.Id);
                }
            }
        }

        // هزینه ارسال
        var shippingCost = zone.DeliveryFee;

        // ایجاد سفارش
        var order = new Order
        {
            UserId = userId,
            OrderNumber = $"FS-{DateTime.UtcNow:yyyyMMddHHmmss}",
            TotalAmount = total,
            DiscountAmount = discountAmount,
            ShippingCost = shippingCost,
            FinalAmount = total - discountAmount + shippingCost,
            DiscountCode = discountCode,
            AddressId = addressId,
            DeliveryZoneId = deliveryZoneId,
            Items = items
        };

        await _orders.AddAsync(order);

        // پاک کردن سبد
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();

        // ارسال پیامک تایید
        var user = await _db.Users.FindAsync(userId);
        if (user?.PhoneNumber != null)
            await _sms.SendOrderConfirmationAsync(user.PhoneNumber, order.OrderNumber, order.FinalAmount);

        return RedirectToAction("Confirmation", new { id = order.Id });
    }

    /// <summary>صفحه تایید سفارش</summary>
    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await _orders.GetByIdAsync(id);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (order == null || (order.UserId != userId && !User.IsInRole("Admin"))) return NotFound();
        return View(order);
    }

    // ─── Helper: دریافت یا ایجاد سبد برای کاربر / مهمان ─────────────────────
    private async Task<Cart> GetOrCreateCartAsync()
    {
        string? userId = null;
        string? sessionId = null;

        if (User.Identity?.IsAuthenticated == true)
            userId = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
        else
        {
            sessionId = HttpContext.Session.GetString("CartSession");
            if (sessionId == null)
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartSession", sessionId);
            }
        }

        var cart = userId != null
            ? await _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(c => c.UserId == userId)
            : await _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId, SessionId = sessionId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        return cart;
    }
}
