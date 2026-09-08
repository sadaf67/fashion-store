using FashionStore.Domain.Common;

namespace FashionStore.Domain.Entities;

/// <summary>سبد خرید</summary>
public class Cart : BaseEntity
{
    public string? UserId { get; set; }        // null برای کاربر مهمان
    public string? SessionId { get; set; }      // برای کاربر مهمان

    public ApplicationUser? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

/// <summary>آیتم سبد خرید</summary>
public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int Quantity { get; set; }

    public Cart? Cart { get; set; }
    public Product? Product { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
