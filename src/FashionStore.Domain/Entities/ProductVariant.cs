using FashionStore.Domain.Common;

namespace FashionStore.Domain.Entities;

/// <summary>واریانت محصول - رنگ و سایز</summary>
public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public string Size { get; set; } = string.Empty;   // S, M, L, XL, XXL
    public string Color { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public int StockQuantity { get; set; }
    public decimal? PriceAdjustment { get; set; }      // تفاوت قیمت نسبت به قیمت پایه

    public Product? Product { get; set; }
}
