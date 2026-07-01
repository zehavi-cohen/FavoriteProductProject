using backend.Data;
using backend.Entities;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace backend.Extensions;

using backend.Authorization;
using backend.Middlewares;

public static class ApplicationServiceExtensions
{
    public const string AngularClientCorsPolicy = "AngularClient";

    public static IServiceCollection AddApplicationDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            );
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        services.AddScoped<TokenService>();
        services.AddScoped<AuthService>();
        services.AddScoped<ProductService>();
        services.AddScoped<AdminService>();

        return services;
    }

    public static IServiceCollection AddApplicationCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException("CORS allowed origins are missing.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(AngularClientCorsPolicy, policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IServiceCollection AddApplicationAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var jwtKey = jwtSection["Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT key is missing.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //בדיקת תקינות יוצר הטוקן
                    ValidateIssuer = true,

                    //בדיקת תקינות נמען הטוקן
                    ValidateAudience = true,

                    //בדיקת תקפות התוקן
                    ValidateLifetime = true,

                    //בדיקת תקינות חתימת הטוקן
                    ValidateIssuerSigningKey = true,

                    //בדיקת תאימות נתוני הטוקן להגדרות שהוגדו בשרת
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],

                    //KEY- הגדרת מפתח לפתחית הטוקן- עם ה
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    public static IServiceCollection AddApplicationAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                policy.RequireRole(AppRoles.Admin));
        });

        return services;
    }

    public static IServiceCollection AddApplicationSwagger(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    public static IApplicationBuilder UseCurrentUser(
    this IApplicationBuilder app)
    {
        return app.UseMiddleware<CurrentUserMiddleware>();
    }
}