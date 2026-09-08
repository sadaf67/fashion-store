using FashionStore.Application.Interfaces;
using FashionStore.Domain.Entities;
using FashionStore.Domain.Enums;
using FashionStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FashionStore.Infrastructure.Repositories;

/// <summary>پیاده‌سازی repository محصولات</summary>
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db) => _db = db;

    public async Task<Product?> GetByIdAsync(int id) =>
        await _db.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Product>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.Products.Include(p => p.Images).Include(p => p.Category).AsQueryable();
        if (!includeInactive)
            query = query.Where(p => p.Status == ProductStatus.Active);
        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId) =>
        await _db.Products
            .Include(p => p.Images)
            .Where(p => p.CategoryId == categoryId && p.Status == ProductStatus.Active)
            .ToListAsync();

    public async Task<IEnumerable<Product>> SearchAsync(string query) =>
        await _db.Products
            .Include(p => p.Images)
            .Where(p => p.Name.Contains(query) || (p.Description != null && p.Description.Contains(query)) || p.Tags.Contains(query))
            .ToListAsync();

    public async Task<IEnumerable<Product>> GetFeaturedAsync(int count = 8) =>
        await _db.Products
            .Include(p => p.Images)
            .Where(p => p.IsFeatured && p.Status == ProductStatus.Active)
            .Take(count)
            .ToListAsync();

    public async Task<Product> AddAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            product.IsDeleted = true; // Soft delete
            await _db.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalCountAsync() =>
        await _db.Products.CountAsync();
}
