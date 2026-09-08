namespace FashionStore.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,       // در انتظار پرداخت
    Paid = 2,          // پرداخت شده
    Processing = 3,    // در حال آماده‌سازی
    Shipped = 4,       // ارسال شده
    Delivered = 5,     // تحویل داده شده
    Cancelled = 6      // لغو شده
}

public enum DiscountType
{
    Percentage = 1,    // درصدی
    FixedAmount = 2    // مقداری ثابت
}

public enum ProductStatus
{
    Active = 1,
    Inactive = 2,
    OutOfStock = 3
}

public enum SmsType
{
    OtpLogin = 1,      // کد ورود
    OrderConfirm = 2,  // تایید سفارش
    OrderShipped = 3,  // ارسال سفارش
    Promotional = 4    // پیام تبلیغاتی
}
