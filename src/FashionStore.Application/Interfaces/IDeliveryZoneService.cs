using FashionStore.Domain.Entities;

namespace FashionStore.Application.Interfaces;

/// <summary>سرویس محدوده تحویل پیک</summary>
public interface IDeliveryZoneService
{
    Task<IEnumerable<DeliveryZone>> GetAllZonesAsync();
    Task<DeliveryZone?> GetZoneByIdAsync(int id);
    Task<DeliveryZone?> GetZoneForAddressAsync(string city, string province);
    Task<DeliveryZone> CreateZoneAsync(DeliveryZone zone);
    Task UpdateZoneAsync(DeliveryZone zone);
    Task DeleteZoneAsync(int id);
}
