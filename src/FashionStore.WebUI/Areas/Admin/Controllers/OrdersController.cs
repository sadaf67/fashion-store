using FashionStore.Application.Interfaces;
using FashionStore.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.WebUI.Areas.Admin.Controllers;

/// <summary>مدیریت سفارشات</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly IOrderRepository _orders;
    private readonly ISmsService _sms;

    public OrdersController(IOrderRepository orders, ISmsService sms)
    {
        _orders = orders;
        _sms = sms;
    }

    public async Task<IActionResult> Index(OrderStatus? status)
    {
        var orders = await _orders.GetAllAsync(status);
        ViewBag.CurrentStatus = status;
        return View(orders);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var order = await _orders.GetByIdAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    /// <summary>تغییر وضعیت سفارش + ارسال پیامک خودکار</summary>
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, string? trackingNumber)
    {
        var order = await _orders.GetByIdAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        if (!string.IsNullOrEmpty(trackingNumber))
            order.TrackingNumber = trackingNumber;

        if (status == OrderStatus.Delivered)
            order.DeliveredAt = DateTime.UtcNow;

        await _orders.UpdateAsync(order);

        // پیامک اتوماتیک هنگام ارسال
        if (status == OrderStatus.Shipped && order.User?.PhoneNumber != null)
            await _sms.SendOrderShippedAsync(order.User.PhoneNumber, order.OrderNumber, trackingNumber);

        TempData["Success"] = "وضعیت سفارش بروزرسانی شد";
        return RedirectToAction(nameof(Detail), new { id });
    }
}
