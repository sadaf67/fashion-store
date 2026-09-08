using FashionStore.Domain.Enums;

namespace FashionStore.Application.Interfaces;

/// <summary>سرویس ارسال پیامک</summary>
public interface ISmsService
{
    Task<bool> SendOtpAsync(string phoneNumber, string code);
    Task<bool> SendOrderConfirmationAsync(string phoneNumber, string orderNumber, decimal amount);
    Task<bool> SendOrderShippedAsync(string phoneNumber, string orderNumber, string? trackingCode);
    Task<bool> SendBulkAsync(IEnumerable<string> phoneNumbers, string message, string? userId = null);
    Task<bool> SendAsync(string phoneNumber, string message, SmsType type, string? userId = null);
}
