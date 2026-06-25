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

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return ServiceResult<AuthResponse>.BadRequest("User name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ServiceResult<AuthResponse>.BadRequest("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResult<AuthResponse>.BadRequest("Password is required.");
        }

        var exists = await _db.AppUsers.AnyAsync(x =>
            x.UserName == request.UserName ||
            x.Email == request.Email);

        if (exists)
        {
            return ServiceResult<AuthResponse>.BadRequest("User name or email already exists.");
        }

        var userRole = await _db.Roles
            .FirstOrDefaultAsync(x => x.Name == "User");

        if (userRole == null)
        {
            return ServiceResult<AuthResponse>.Problem("Role 'User' was not found in database.");
        }

        var user = new AppUser
        {
            UserName = request.UserName,
            Email = request.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

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

        var response = CreateAuthResponse(userWithRoles);

        return ServiceResult<AuthResponse>.Success(response);
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail))
        {
            return ServiceResult<AuthResponse>.BadRequest("User name or email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResult<AuthResponse>.BadRequest("Password is required.");
        }

        var user = await _db.AppUsers
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.UserName == request.UserNameOrEmail ||
                x.Email == request.UserNameOrEmail);

        if (user == null)
        {
            return ServiceResult<AuthResponse>.Unauthorized();
        }

        if (!user.IsActive)
        {
            return ServiceResult<AuthResponse>.BadRequest("User is not active.");
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return ServiceResult<AuthResponse>.Unauthorized();
        }

        var response = CreateAuthResponse(user);

        return ServiceResult<AuthResponse>.Success(response);
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