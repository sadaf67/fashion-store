using FashionStore.Application.Interfaces;
using FashionStore.Domain.Entities;
using FashionStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.Infrastructure.Services;

/// <summary>سرویس محدوده‌بندی پیک</summary>
public class DeliveryZoneService : IDeliveryZoneService
{
    private readonly ApplicationDbContext _db;

    public DeliveryZoneService(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<DeliveryZone>> GetAllZonesAsync() =>
        await _db.DeliveryZones.Where(z => !z.IsDeleted).OrderBy(z => z.Name).ToListAsync();

    public async Task<DeliveryZone?> GetZoneByIdAsync(int id) =>
        await _db.DeliveryZones.FindAsync(id);

    /// <summary>یافتن زون تحویل بر اساس شهر و استان</summary>
    public async Task<DeliveryZone?> GetZoneForAddressAsync(string city, string province)
    {
        // منطق ساده: بر اساس شهر تهران / غیر تهران
        if (city.Contains("تهران"))
            return await _db.DeliveryZones.FirstOrDefaultAsync(z => z.IsActive && z.Name.Contains("تهران - داخل"));

        if (province.Contains("تهران"))
            return await _db.DeliveryZones.FirstOrDefaultAsync(z => z.IsActive && z.Name.Contains("حومه"));

        return await _db.DeliveryZones.FirstOrDefaultAsync(z => z.IsActive && z.Name.Contains("سایر"));
    }

    public async Task<DeliveryZone> CreateZoneAsync(DeliveryZone zone)
    {
        _db.DeliveryZones.Add(zone);
        await _db.SaveChangesAsync();
        return zone;
    }

    public async Task UpdateZoneAsync(DeliveryZone zone)
    {
        zone.UpdatedAt = DateTime.UtcNow;
        _db.DeliveryZones.Update(zone);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteZoneAsync(int id)
    {
        var zone = await _db.DeliveryZones.FindAsync(id);
        if (zone != null) { zone.IsDeleted = true; await _db.SaveChangesAsync(); }
    }
}
