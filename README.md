# 👗 FashionStore - فروشگاه آنلاین پوشاک

## 🚀 اجرای سریع

```bash
cd D:\FashionStore\src\FashionStore.WebUI
dotnet run
```

بعد از اجرا، مرورگر را باز کنید: **https://localhost:5001**

---

## 🔐 اطلاعات ورود مدیر

| فیلد | مقدار |
|------|-------|
| ایمیل | admin@fashionstore.com |
| رمز عبور | Admin@123 |
| پنل مدیریت | https://localhost:5001/Admin/Dashboard |

---

## 📁 ساختار پروژه

```
FashionStore/
├── src/
│   ├── FashionStore.Domain/          # Entities, Enums
│   ├── FashionStore.Application/     # Interfaces, DTOs, Services
│   ├── FashionStore.Infrastructure/  # EF Core, Repositories, SMS
│   └── FashionStore.WebUI/           # MVC Controllers + Views
│       ├── Areas/Admin/              # پنل مدیریت
│       ├── Controllers/              # صفحات عمومی
│       ├── Views/                    # HTML Views
│       └── wwwroot/css,js            # استایل و اسکریپت
```

---

## ✅ امکانات

| امکان | توضیح |
|-------|-------|
| 🛍️ فروشگاه | لیست، جستجو، فیلتر و جزئیات محصول |
| 📷 پرو آنلاین | Virtual Try-On با دوربین (MediaPipe) |
| 🛒 سبد خرید | افزودن، حذف، تخفیف، پیک |
| 👤 ثبت نام | ثبت نام، ورود، پروفایل |
| 📊 داشبورد | آمار، نمودار درآمد، سفارشات اخیر |
| 👗 محصولات | CRUD کامل + واریانت رنگ/سایز + آپلود تصویر |
| 📦 سفارشات | مدیریت و تغییر وضعیت + پیامک خودکار |
| 🎫 تخفیف | کدهای درصدی/مقداری با تاریخ انقضا |
| 📱 پیامک | ارسال فردی/انبوه + لاگ |
| 🚚 پیک | محدوده‌بندی با هزینه متفاوت |
| ⚙️ تنظیمات | همه چیز داینامیک از پنل |

---

## 🎨 تم

- **کرم** `#F5F0E8` و **آبی روشن** `#6BA3BE`  
- CSS Variables در `wwwroot/css/site.css`
- فونت فارسی Vazirmatn
