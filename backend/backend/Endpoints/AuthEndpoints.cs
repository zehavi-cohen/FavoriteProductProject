using backend.DTOs.Auth;
using backend.Services;
using backend.Extensions;

namespace backend.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", RegisterAsync);

        group.MapPost("/login", LoginAsync);

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        AuthService authService)
    {
        var result = await authService.RegisterAsync(request);

        return result.ToHttpResult();
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AuthService authService)
    {
        var result = await authService.LoginAsync(request);

        return result.ToHttpResult();
    }
}