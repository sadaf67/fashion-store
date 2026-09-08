using FashionStore.Application.Interfaces;
using FashionStore.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.WebUI.Areas.Admin.Controllers;

/// <summary>مدیریت محدوده پیک</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DeliveryController : Controller
{
    private readonly IDeliveryZoneService _deliveryService;

    public DeliveryController(IDeliveryZoneService deliveryService) => _deliveryService = deliveryService;

    public async Task<IActionResult> Index() =>
        View(await _deliveryService.GetAllZonesAsync());

    [HttpGet]
    public IActionResult Create() => View(new DeliveryZone());

    [HttpPost]
    public async Task<IActionResult> Create(DeliveryZone model)
    {
        await _deliveryService.CreateZoneAsync(model);
        TempData["Success"] = "محدوده پیک اضافه شد";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var zone = await _deliveryService.GetZoneByIdAsync(id);
        if (zone == null) return NotFound();
        return View(zone);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(DeliveryZone model)
    {
        await _deliveryService.UpdateZoneAsync(model);
        TempData["Success"] = "محدوده بروزرسانی شد";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _deliveryService.DeleteZoneAsync(id);
        TempData["Success"] = "محدوده حذف شد";
        return RedirectToAction(nameof(Index));
    }
}
