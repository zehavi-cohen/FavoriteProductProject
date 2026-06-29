using backend.Data;
using backend.DTOs.Admin;
using backend.DTOs.Auth;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AdminService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AdminService(
      AppDbContext db,
      TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
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

    public async Task<ServiceResult<AuthResponse>> LoginAsUserAsync(
    int userId,
    int adminUserId)
    {
        if (userId == adminUserId)
        {
            return ServiceResult<AuthResponse>.BadRequest("Admin cannot login as himself.");
        }

        var adminUser = await _db.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == adminUserId);

        if (adminUser == null)
        {
            return ServiceResult<AuthResponse>.Unauthorized();
        }

        if (!adminUser.IsActive)
        {
            return ServiceResult<AuthResponse>.BadRequest("Admin user is not active.");
        }

        var targetUser = await _db.AppUsers
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (targetUser == null)
        {
            return ServiceResult<AuthResponse>.NotFound("User was not found.");
        }

        if (!targetUser.IsActive)
        {
            return ServiceResult<AuthResponse>.BadRequest("User is not active.");
        }

        var targetUserIsAdmin = targetUser.UserRoles
            .Any(x => x.Role.Name == "Admin");

        if (targetUserIsAdmin)
        {
            return ServiceResult<AuthResponse>.BadRequest("Cannot login as another admin user.");
        }

        var token = _tokenService.CreateToken(
            targetUser,
            adminUser.Id,
            adminUser.UserName);

        var response = new AuthResponse
        {
            UserId = targetUser.Id,
            UserName = targetUser.UserName,
            Email = targetUser.Email,
            Roles = targetUser.UserRoles
                .Select(x => x.Role.Name)
                .ToList(),
            Token = token,
            IsImpersonating = true,
            ImpersonatedByUserId = adminUser.Id,
            ImpersonatedByUserName = adminUser.UserName
        };

        return ServiceResult<AuthResponse>.Success(response);
    }
}