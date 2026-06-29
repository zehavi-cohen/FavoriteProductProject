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

    public async Task<List<AdminUserDto>> GetUsersAsync()
    {
        return await _db.AppUsers
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
    }

    public async Task<LoginAsUserResult> LoginAsUserAsync(
        int userId,
        int adminUserId)
    {
        if (userId == adminUserId)
        {
            return new LoginAsUserResult(LoginAsUserStatus.SameUser);
        }

        var adminUser = await _db.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == adminUserId);

        if (adminUser == null)
        {
            return new LoginAsUserResult(LoginAsUserStatus.AdminNotFound);
        }

        if (!adminUser.IsActive)
        {
            return new LoginAsUserResult(LoginAsUserStatus.AdminInactive);
        }

        var targetUser = await _db.AppUsers
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (targetUser == null)
        {
            return new LoginAsUserResult(LoginAsUserStatus.TargetUserNotFound);
        }

        if (!targetUser.IsActive)
        {
            return new LoginAsUserResult(LoginAsUserStatus.TargetUserInactive);
        }

        var targetUserIsAdmin = targetUser.UserRoles
            .Any(x => x.Role.Name == "Admin");

        if (targetUserIsAdmin)
        {
            return new LoginAsUserResult(LoginAsUserStatus.TargetUserIsAdmin);
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

        return new LoginAsUserResult(
            LoginAsUserStatus.Success,
            response);
    }
}

public record LoginAsUserResult(
    LoginAsUserStatus Status,
    AuthResponse? AuthResponse = null);

public enum LoginAsUserStatus
{
    Success,
    SameUser,
    AdminNotFound,
    AdminInactive,
    TargetUserNotFound,
    TargetUserInactive,
    TargetUserIsAdmin
}