using FashionStore.Application.Interfaces;
using FashionStore.Domain.Entities;
using FashionStore.Domain.Enums;
using FashionStore.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FashionStore.Infrastructure.Services;

/// <summary>سرویس پیامک شبیه‌سازی - برای توسعه و تست</summary>
public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;
    private readonly ApplicationDbContext _db;

    public MockSmsService(ILogger<MockSmsService> logger, ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string code)
    {
        var message = $"کد ورود شما: {code}\nاین کد ۵ دقیقه اعتبار دارد.";
        return await SendAsync(phoneNumber, message, SmsType.OtpLogin);
    }

    public async Task<bool> SendOrderConfirmationAsync(string phoneNumber, string orderNumber, decimal amount)
    {
        var message = $"سفارش شما به شماره {orderNumber} با مبلغ {amount:N0} تومان ثبت شد.";
        return await SendAsync(phoneNumber, message, SmsType.OrderConfirm);
    }

    public async Task<bool> SendOrderShippedAsync(string phoneNumber, string orderNumber, string? trackingCode)
    {
        var message = $"سفارش {orderNumber} ارسال شد. کد رهگیری: {trackingCode ?? "---"}";
        return await SendAsync(phoneNumber, message, SmsType.OrderShipped);
    }

    public async Task<bool> SendBulkAsync(IEnumerable<string> phoneNumbers, string message, string? userId = null)
    {
        var results = true;
        foreach (var phone in phoneNumbers)
            results &= await SendAsync(phone, message, SmsType.Promotional, userId);
        return results;
    }

    public async Task<bool> SendAsync(string phoneNumber, string message, SmsType type, string? userId = null)
    {
        // در محیط توسعه فقط log می‌کنیم
        _logger.LogInformation("[SMS Mock] To: {Phone} | Type: {Type} | Message: {Message}", phoneNumber, type, message);

        // ذخیره لاگ در دیتابیس
        _db.SmsLogs.Add(new SmsLog
        {
            PhoneNumber = phoneNumber,
            Message = message,
            Type = type,
            IsSuccess = true,
            ProviderResponse = "MOCK_SUCCESS",
            UserId = userId
        });
        await _db.SaveChangesAsync();

        return true;
    }
}
