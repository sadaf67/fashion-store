using FashionStore.Domain.Common;

namespace FashionStore.Domain.Entities;

/// <summary>تصاویر محصول</summary>
public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }

    public Product? Product { get; set; }
}
