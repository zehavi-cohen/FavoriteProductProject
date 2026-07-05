using backend.Data;
using backend.DTOs.Admin;
using backend.DTOs.Auth;
using backend.Contexts.CurrentUser;
using backend.Authentication;
using backend.Authorization;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AdminService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly ICurrentUserContext _currentUser;

    public AdminService(
    AppDbContext db,
    TokenService tokenService,
    ICurrentUserContext currentUser)
    {
        _db = db;
        _tokenService = tokenService;
        _currentUser = currentUser;
    }

    public async Task<List<AdminUserDto>> GetUsersAsync()
    {
        return await _db.AppUsers
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .OrderBy(x => x.UserName)
            .Select(x => new AdminUserDto
            (
                UserId: x.Id,
                UserName: x.UserName,
                Email: x.Email,
                IsActive: x.IsActive,
                Roles: x.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList()
            ))
            .ToListAsync();
    }

    public async Task<LoginAsUserResult> LoginAsUserAsync(int userId)
    {
        var adminUserId = _currentUser.GetRequiredUserId();

        if (userId == adminUserId)
        {
            return new LoginAsUserResult(LoginAsUserStatus.SameUser);
        }

        var adminUser = await _db.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == adminUserId);

        if (adminUser is null)
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

        if (targetUser is null)
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

        var accessToken = _tokenService.CreateToken(
            targetUser,
            adminUser.Id,
            adminUser.UserName);

        var response = new AuthResponse(
            UserId: targetUser.Id,
            UserName: targetUser.UserName,
            Email: targetUser.Email,
            Roles: targetUser.UserRoles
                .Select(x => x.Role.Name)
                .ToList(),
            IsImpersonating: true,
            ImpersonatedByUserId: adminUser.Id,
            ImpersonatedByUserName: adminUser.UserName
        );

        var authenticationResult = new AuthenticationResult(
                Response: response,
                AccessToken: accessToken
            );

        return new LoginAsUserResult(
            LoginAsUserStatus.Success,
            authenticationResult);
    }
    public async Task<StopImpersonationResult> StopImpersonationAsync()
    {
        if (!_currentUser.IsImpersonating ||
            _currentUser.ImpersonatedByUserId is null)
        {
            return new StopImpersonationResult(
                StopImpersonationStatus.NotImpersonating);
        }

        var adminUserId = _currentUser.ImpersonatedByUserId.Value;

        var adminUser = await _db.AppUsers
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == adminUserId);

        if (adminUser is null)
        {
            return new StopImpersonationResult(
                StopImpersonationStatus.OriginalAdminNotFound);
        }

        if (!adminUser.IsActive)
        {
            return new StopImpersonationResult(
                StopImpersonationStatus.OriginalAdminInactive);
        }

        var token = _tokenService.CreateToken(adminUser);

        var response = new AuthResponse(
            UserId: adminUser.Id,
            UserName: adminUser.UserName,
            Email: adminUser.Email,
            Roles: adminUser.UserRoles
                .Select(x => x.Role.Name)
                .ToList()
        );

        var authenticationResult = new AuthenticationResult(
            Response: response,
            AccessToken: token
        );

        return new StopImpersonationResult(
            StopImpersonationStatus.Success,
            authenticationResult);
    }

    public record LoginAsUserResult(
    LoginAsUserStatus Status,
    AuthenticationResult? AuthResult = null);

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

    public record StopImpersonationResult(
    StopImpersonationStatus Status,
    AuthenticationResult? AuthResult = null);

    public enum StopImpersonationStatus
    {
        Success,
        NotImpersonating,
        OriginalAdminNotFound,
        OriginalAdminInactive
    }
}