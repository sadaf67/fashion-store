using FashionStore.Domain.Common;
using FashionStore.Domain.Enums;

namespace FashionStore.Domain.Entities;

/// <summary>لاگ پیامک‌های ارسال شده</summary>
public class SmsLog : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public SmsType Type { get; set; }
    public bool IsSuccess { get; set; }
    public string? ProviderResponse { get; set; }
    public string? UserId { get; set; }

    public ApplicationUser? User { get; set; }
}

/// <summary>قالب پیامک تعریف شده توسط مدیر</summary>
public class SmsTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public SmsType Type { get; set; }
    public string Template { get; set; } = string.Empty; // متن با متغیرهای {name}, {code} و...
    public bool IsActive { get; set; } = true;
}
