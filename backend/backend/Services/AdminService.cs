using backend.Data;
using backend.DTOs.Admin;
using backend.DTOs.Products;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AdminService
{
    private readonly AppDbContext _db;
    private readonly ProductService _productService;

    public AdminService(
        AppDbContext db,
        ProductService productService)
    {
        _db = db;
        _productService = productService;
    }

    public async Task<ServiceResult<List<AdminUserDto>>> GetUsersAsync()
    {
        var users = await _db.AppUsers
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .OrderBy(x => x.UserName)
            .Select(x => new AdminUserDto
            {
                UserId = x.Id,
                UserName = x.UserName,
                Email = x.Email,
                IsActive = x.IsActive,
                Roles = x.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList()
            })
            .ToListAsync();

        return ServiceResult<List<AdminUserDto>>.Success(users);
    }

    public async Task<ServiceResult<List<ProductDto>>> GetUserProductsAsync(int userId)
    {
        var userExists = await _db.AppUsers
            .AnyAsync(x => x.Id == userId);

        if (!userExists)
        {
            return ServiceResult<List<ProductDto>>.NotFound("User was not found.");
        }

        var products = await _productService.GetProductsForUserAsync(userId);

        return ServiceResult<List<ProductDto>>.Success(products);
    }

    public async Task<ServiceResult<bool>> SetFavoriteForUserAsync(
        int userId,
        int productId,
        bool isFavorite)
    {
        var userExists = await _db.AppUsers
            .AnyAsync(x => x.Id == userId);

        if (!userExists)
        {
            return ServiceResult<bool>.NotFound("User was not found.");
        }

        var updated = await _productService.SetFavoriteAsync(
            userId,
            productId,
            isFavorite);

        if (!updated)
        {
            return ServiceResult<bool>.NotFound("Product was not found.");
        }

        return ServiceResult<bool>.Success(true);
    }
}