using backend.Authentication;
using backend.Authorization;
using backend.Data;
using backend.DTOs.Auth;
using backend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly TokenService _tokenService;

    public AuthService(
        AppDbContext db,
        IPasswordHasher<AppUser> passwordHasher,
        TokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    private async Task<bool> UserExistsAsync(
    string userName,
    string email)
    {
        return await _db.AppUsers.AnyAsync(x =>
            x.UserName == userName ||
            x.Email == email);
    }

    public async Task<AuthenticationResult?> RegisterAsync(RegisterRequest request)
    {
        var userExists = await UserExistsAsync(
            request.UserName,
            request.Email);

        if (userExists)
        {
            return null;
        }
        var userRole = await _db.Roles
            .FirstOrDefaultAsync(x => x.Name == AppRoles.User);

        if (userRole == null)
        {
            throw new InvalidOperationException("Role 'User' was not found in database.");
        }

        var user = new AppUser
        {
            UserName = request.UserName,
            Email = request.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        user.UserRoles.Add(new AppUserRole
        {
            User = user,
            Role = userRole,
            CreatedAt = DateTime.UtcNow
        });

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();

        var userWithRoles = await _db.AppUsers
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstAsync(x => x.Id == user.Id);

        return CreateAuthenticationResult(userWithRoles);
    }

    public async Task<AuthenticationResult?> LoginAsync(LoginRequest request)
    {
        var user = await _db.AppUsers
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.UserName == request.UserNameOrEmail ||
                x.Email == request.UserNameOrEmail);

        if (user == null)
        {
            return null;
        }

        if (!user.IsActive)
        {
            return null;
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return CreateAuthenticationResult(user);
    }

    private AuthenticationResult CreateAuthenticationResult(AppUser user)
    {
        var accessToken = _tokenService.CreateToken(user);

        var response = new AuthResponse(
            UserId: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Roles: user.UserRoles
                .Select(x => x.Role.Name)
                .ToList()
        );

        return new AuthenticationResult(
            Response: response,
            AccessToken: accessToken
        );
    }
}