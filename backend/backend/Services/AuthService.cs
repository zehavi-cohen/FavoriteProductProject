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

    public async Task<bool> UserExistsAsync(
        string userName,
        string email)
    {
        return await _db.AppUsers.AnyAsync(x =>
            x.UserName == userName ||
            x.Email == email);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var userRole = await _db.Roles
            .FirstOrDefaultAsync(x => x.Name == "User");

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

        return CreateAuthResponse(userWithRoles);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
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

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(AppUser user)
    {
        var token = _tokenService.CreateToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = user.UserRoles
                .Select(x => x.Role.Name)
                .ToList(),
            Token = token
        };
    }
}