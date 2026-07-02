using backend.Data;
using backend.DTOs.Products;
using backend.Entities;
using backend.Contexts.CurrentUser;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProductService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public ProductService(AppDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<ProductDto>> GetMyProductsAsync()
    {
        var userId = _currentUser.GetRequiredUserId();
        var products = await _db.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto
            (
                ProductId : p.Id,
                ProductName : p.Name,
                Code: p.Code,
                Description: p.Description,
                IsFavorite : _db.UserFavoriteProducts.Any(ufp =>
                    ufp.UserId == userId &&
                    ufp.ProductId == p.Id)
            ))
            .ToListAsync();

        return products;
    }

    public async Task<bool> SetFavoriteAsync(int productId, bool isFavorite)
    {
        var userId = _currentUser.GetRequiredUserId();
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