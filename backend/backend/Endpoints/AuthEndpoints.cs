using backend.Contexts.CurrentUser;
using backend.DTOs.Auth;
using backend.Extensions;
using backend.Services;
using FluentValidation;
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

        group.MapPost("/logout", LogoutAsync);

        group.MapGet("/me", GetMeAsync)
            .RequireAuthorization();

        return app;
    }

    private static async Task<Results<Ok<AuthResponse>, ValidationProblem, BadRequest<string>, ProblemHttpResult>> RegisterAsync(
        RegisterRequest request,
        IValidator<RegisterRequest> validator,
        AuthService authService,
        HttpResponse httpResponse,
        IConfiguration configuration)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(
                validationResult.ToValidationProblemDictionary()
            );
        }

        try
        {
            var result = await authService.RegisterAsync(request);

            if (result is null)
            {
                return TypedResults.BadRequest("User name or email already exists.");
            }

            httpResponse.AppendAuthCookie(
                result.AccessToken,
                configuration);

            return TypedResults.Ok(result.Response);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Problem(ex.Message);
        }
    }

    private static async Task<Results<Ok<AuthResponse>, ValidationProblem, UnauthorizedHttpResult>> LoginAsync(
        LoginRequest request,
        IValidator<LoginRequest> validator,
        AuthService authService,
        HttpResponse httpResponse,
        IConfiguration configuration)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(
                validationResult.ToValidationProblemDictionary()
            );
        }

        var result = await authService.LoginAsync(request);

        if (result is null)
        {
            return TypedResults.Unauthorized();
        }

        httpResponse.AppendAuthCookie(
            result.AccessToken,
            configuration);

        return TypedResults.Ok(result.Response);
    }

    private static NoContent LogoutAsync(HttpResponse httpResponse)
    {
        httpResponse.DeleteAuthCookie();

        return TypedResults.NoContent();
    }

    private static Ok<AuthResponse> GetMeAsync(
    ICurrentUserContext currentUser)
    {
        var response = new AuthResponse(
            UserId: currentUser.GetRequiredUserId(),
            UserName: currentUser.UserName ?? string.Empty,
            Email: currentUser.Email ?? string.Empty,
            Roles: currentUser.Roles.ToList(),
            IsImpersonating: currentUser.IsImpersonating,
            ImpersonatedByUserId: currentUser.ImpersonatedByUserId,
            ImpersonatedByUserName: currentUser.ImpersonatedByUserName
        );

        return TypedResults.Ok(response);
    }
}