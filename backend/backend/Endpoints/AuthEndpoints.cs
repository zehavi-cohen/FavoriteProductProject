using backend.DTOs.Auth;
using backend.Services;
using Microsoft.AspNetCore.Http.HttpResults;

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

    private static async Task<Results<Ok<AuthResponse>, BadRequest<string>, ProblemHttpResult>> RegisterAsync(
        RegisterRequest request,
        AuthService authService)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return TypedResults.BadRequest("User name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return TypedResults.BadRequest("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return TypedResults.BadRequest("Password is required.");
        }

        var userExists = await authService.UserExistsAsync(
            request.UserName,
            request.Email);

        if (userExists)
        {
            return TypedResults.BadRequest("User name or email already exists.");
        }

        try
        {
            var response = await authService.RegisterAsync(request);

            return TypedResults.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Problem(ex.Message);
        }
    }

    private static async Task<Results<Ok<AuthResponse>, BadRequest<string>, UnauthorizedHttpResult>> LoginAsync(
        LoginRequest request,
        AuthService authService)
    {
        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail))
        {
            return TypedResults.BadRequest("User name or email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return TypedResults.BadRequest("Password is required.");
        }

        var response = await authService.LoginAsync(request);

        if (response == null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(response);
    }
}