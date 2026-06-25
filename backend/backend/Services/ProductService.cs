using backend.Data;
using backend.DTOs.Products;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductDto>> GetProductsForUserAsync(int userId)
    {
        var products = await _db.Products
            .Where(p => p.IsActive)
            .Select(p => new ProductDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Code = p.Code,
                Description = p.Description,
                IsFavorite = _db.UserFavoriteProducts.Any(ufp =>
                    ufp.UserId == userId &&
                    ufp.ProductId == p.Id)
            })
            .OrderBy(p => p.ProductName)
            .ToListAsync();

        return products;
    }

    public async Task<bool> SetFavoriteAsync(int userId, int productId, bool isFavorite)
    {
        var productExists = await _db.Products
            .AnyAsync(p => p.Id == productId && p.IsActive);

        if (!productExists)
        {
            return false;
        }

        var existingFavorite = await _db.UserFavoriteProducts
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.ProductId == productId);

        if (isFavorite)
        {
            if (existingFavorite == null)
            {
                _db.UserFavoriteProducts.Add(new UserFavoriteProduct
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        else
        {
            if (existingFavorite != null)
            {
                _db.UserFavoriteProducts.Remove(existingFavorite);
            }
        }

        await _db.SaveChangesAsync();

        return true;
    }
}