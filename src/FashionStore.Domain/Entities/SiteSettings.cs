using FashionStore.Domain.Common;

namespace FashionStore.Domain.Entities;

/// <summary>تنظیمات سایت - مدیر می‌تواند همه چیز را از پنل تغییر دهد</summary>
public class SiteSettings : BaseEntity
{
    public string SiteName { get; set; } = "فروشگاه مد";
    public string? Logo { get; set; }
    public string? Favicon { get; set; }
    public string? HeroTitle { get; set; }
    public string? HeroSubtitle { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TelegramUrl { get; set; }
    public string? AboutText { get; set; }
    public bool IsMaintenanceMode { get; set; } = false;
    public string? SmsApiKey { get; set; }      // کلید API پیامک
    public string? SmsSender { get; set; }      // خط ارسال
}
