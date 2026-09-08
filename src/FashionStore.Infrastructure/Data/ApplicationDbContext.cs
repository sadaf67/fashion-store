using FashionStore.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.Infrastructure.Data;

/// <summary>DbContext اصلی برنامه</summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<DeliveryZone> DeliveryZones => Set<DeliveryZone>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<SmsLog> SmsLogs => Set<SmsLog>();
    public DbSet<SmsTemplate> SmsTemplates => Set<SmsTemplate>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // فیلتر Soft Delete برای همه entity ها
        builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        builder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);

        // پیکربندی روابط
        builder.Entity<Product>()
            .HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Product>()
            .HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // SQL Server permits only one cascade route to Orders.
        builder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Order>()
            .HasOne(o => o.Address)
            .WithMany(a => a.Orders)
            .HasForeignKey(o => o.AddressId)
            .OnDelete(DeleteBehavior.NoAction);

        // Precision برای قیمت
        builder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 0);
        builder.Entity<Product>().Property(p => p.DiscountedPrice).HasPrecision(18, 0);
        builder.Entity<ProductVariant>().Property(p => p.PriceAdjustment).HasPrecision(18, 0);
        builder.Entity<DeliveryZone>().Property(p => p.DeliveryFee).HasPrecision(18, 0);
        builder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 0);
        builder.Entity<Order>().Property(o => o.DiscountAmount).HasPrecision(18, 0);
        builder.Entity<Order>().Property(o => o.ShippingCost).HasPrecision(18, 0);
        builder.Entity<Order>().Property(o => o.FinalAmount).HasPrecision(18, 0);
        builder.Entity<OrderItem>().Property(o => o.UnitPrice).HasPrecision(18, 0);
        builder.Entity<OrderItem>().Property(o => o.TotalPrice).HasPrecision(18, 0);
        builder.Entity<Discount>().Property(d => d.Value).HasPrecision(18, 2);

        // Index ها
        builder.Entity<Discount>().HasIndex(d => d.Code).IsUnique();
        builder.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique();

        // Seed داده اولیه
        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        // تنظیمات اولیه سایت
        builder.Entity<SiteSettings>().HasData(new SiteSettings
        {
            Id = 1,
            SiteName = "فروشگاه مد الگانت",
            HeroTitle = "جدیدترین مدهای روز",
            HeroSubtitle = "با بهترین کیفیت و قیمت مناسب",
            ContactPhone = "021-12345678"
        });

        // دسته‌بندی های اولیه
        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "لباس زنانه", SortOrder = 1, IsActive = true },
            new Category { Id = 2, Name = "لباس مردانه", SortOrder = 2, IsActive = true },
            new Category { Id = 3, Name = "لباس بچگانه", SortOrder = 3, IsActive = true },
            new Category { Id = 4, Name = "اکسسوری", SortOrder = 4, IsActive = true }
        );

        // زون‌های تحویل اولیه
        builder.Entity<DeliveryZone>().HasData(
            new DeliveryZone { Id = 1, Name = "تهران - داخل شهر", DeliveryFee = 30000, EstimatedDays = 1, IsActive = true },
            new DeliveryZone { Id = 2, Name = "حومه تهران", DeliveryFee = 50000, EstimatedDays = 2, IsActive = true },
            new DeliveryZone { Id = 3, Name = "سایر استان‌ها", DeliveryFee = 80000, EstimatedDays = 4, IsActive = true }
        );
    }
}
