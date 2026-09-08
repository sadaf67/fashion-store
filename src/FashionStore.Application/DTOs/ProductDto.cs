using FashionStore.Domain.Enums;

namespace FashionStore.Application.DTOs;

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string? CategoryName { get; set; }
    public string? Brand { get; set; }
    public bool IsTryOnEnabled { get; set; }
    public ProductStatus Status { get; set; }
}

public class ProductDetailDto : ProductListDto
{
    public string? Description { get; set; }
    public string Tags { get; set; } = string.Empty;
    public string? TryOnOverlayImageUrl { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<string> AvailableSizes { get; set; } = new();
    public List<string> AvailableColors { get; set; } = new();
}

public class ProductImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class ProductVariantDto
{
    public int Id { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public int StockQuantity { get; set; }
    public decimal? PriceAdjustment { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string? Brand { get; set; }
    public bool IsTryOnEnabled { get; set; } = true;
    public string Tags { get; set; } = string.Empty;
}
