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

        return app;
    }

    private static async Task<Results<Ok<AuthResponse>, ValidationProblem, BadRequest<string>, ProblemHttpResult>> RegisterAsync(
    RegisterRequest request,
    IValidator<RegisterRequest> validator,
    AuthService authService)
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
            var response = await authService.RegisterAsync(request);

            if (response is null)
            {
                return TypedResults.BadRequest("User name or email already exists.");
            }

            return TypedResults.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Problem(ex.Message);
        }
    }

    private static async Task<Results<Ok<AuthResponse>, ValidationProblem, UnauthorizedHttpResult>> LoginAsync(
        LoginRequest request,
        IValidator<LoginRequest> validator,
        AuthService authService)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(
                validationResult.ToValidationProblemDictionary()
            );
        }

        var response = await authService.LoginAsync(request);

        if (response == null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(response);
    }
}