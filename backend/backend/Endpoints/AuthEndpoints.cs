using backend.DTOs.Auth;
using backend.Services;

namespace backend.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            AuthService authService) =>
        {
            var result = await authService.RegisterAsync(request);

            return ToHttpResult(result);
        });

        group.MapPost("/login", async (
            LoginRequest request,
            AuthService authService) =>
        {
            var result = await authService.LoginAsync(request);

            return ToHttpResult(result);
        });

        return app;
    }

    private static IResult ToHttpResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Data);
        }

        return result.StatusCode switch
        {
            StatusCodes.Status400BadRequest => Results.BadRequest(result.ErrorMessage),
            StatusCodes.Status401Unauthorized => Results.Unauthorized(),
            StatusCodes.Status404NotFound => Results.NotFound(result.ErrorMessage),
            _ => Results.Problem(result.ErrorMessage)
        };
    }
}