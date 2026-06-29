using backend.DTOs.Admin;
using backend.DTOs.Auth;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace backend.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/users", GetUsersAsync);

        group.MapPost("/users/{userId:int}/login-as", LoginAsUserAsync);

        return app;
    }

    private static async Task<Ok<List<AdminUserDto>>> GetUsersAsync(
        AdminService adminService)
    {
        var users = await adminService.GetUsersAsync();

        return TypedResults.Ok(users);
    }

    private static async Task<Results<Ok<AuthResponse>, BadRequest<string>, UnauthorizedHttpResult, NotFound<string>, ProblemHttpResult>> LoginAsUserAsync(
        int userId,
        HttpContext httpContext,
        AdminService adminService)
    {
        var adminUserId = httpContext.User.GetUserId();

        var result = await adminService.LoginAsUserAsync(
            userId,
            adminUserId);

        return result.Status switch
        {
            LoginAsUserStatus.Success when result.AuthResponse != null =>
                TypedResults.Ok(result.AuthResponse),

            LoginAsUserStatus.SameUser =>
                TypedResults.BadRequest("Admin cannot login as himself."),

            LoginAsUserStatus.AdminNotFound =>
                TypedResults.Unauthorized(),

            LoginAsUserStatus.AdminInactive =>
                TypedResults.BadRequest("Admin user is not active."),

            LoginAsUserStatus.TargetUserNotFound =>
                TypedResults.NotFound("User was not found."),

            LoginAsUserStatus.TargetUserInactive =>
                TypedResults.BadRequest("User is not active."),

            LoginAsUserStatus.TargetUserIsAdmin =>
                TypedResults.BadRequest("Cannot login as another admin user."),

            _ =>
                TypedResults.Problem("Login as user failed.")
        };
    }
}