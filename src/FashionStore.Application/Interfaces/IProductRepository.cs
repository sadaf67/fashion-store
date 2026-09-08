using FashionStore.Domain.Entities;

namespace FashionStore.Application.Interfaces;

/// <summary>قرارداد repository محصولات</summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> SearchAsync(string query);
    Task<IEnumerable<Product>> GetFeaturedAsync(int count = 8);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<int> GetTotalCountAsync();
}
