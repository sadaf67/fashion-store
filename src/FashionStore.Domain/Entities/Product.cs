using FashionStore.Domain.Common;
using FashionStore.Domain.Enums;

namespace FashionStore.Domain.Entities;

/// <summary>محصول پوشاک</summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public bool IsFeatured { get; set; }
    public bool IsTryOnEnabled { get; set; } = true; // پشتیبانی پرو آنلاین
    public string? TryOnOverlayImageUrl { get; set; } // تصویر overlay برای پرو
    public string Tags { get; set; } = string.Empty; // جدا شده با کاما

    public Category? Category { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
