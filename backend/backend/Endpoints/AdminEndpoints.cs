using backend.DTOs.Admin;
using backend.DTOs.Auth;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using static backend.Services.AdminService;

namespace backend.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAdmin();

        adminGroup.MapGet("/users", GetUsersAsync);

        adminGroup.MapPost("/users/{userId:int}/login-as", LoginAsUserAsync);

        var impersonationGroup = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization();

        impersonationGroup.MapPost("/stop-impersonation", StopImpersonationAsync);

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
    AdminService adminService,
    HttpResponse httpResponse,
    IConfiguration configuration)
    {
        var result = await adminService.LoginAsUserAsync(userId);

        if (result.Status == LoginAsUserStatus.Success &&
            result.AuthResult is not null)
        {
            httpResponse.AppendAuthCookie(
                result.AuthResult.AccessToken,
                configuration);

            return TypedResults.Ok(result.AuthResult.Response);
        }

        return result.Status switch
        {
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
    private static async Task<Results<Ok<AuthResponse>, BadRequest<string>, UnauthorizedHttpResult, ProblemHttpResult>> StopImpersonationAsync(
    AdminService adminService,
    HttpResponse httpResponse,
    IConfiguration configuration)
    {
        var result = await adminService.StopImpersonationAsync();

        if (result.Status == StopImpersonationStatus.Success &&
            result.AuthResult is not null)
        {
            httpResponse.AppendAuthCookie(
                result.AuthResult.AccessToken,
                configuration);

            return TypedResults.Ok(result.AuthResult.Response);
        }

        if (result.Status == StopImpersonationStatus.NotImpersonating)
        {
            return TypedResults.BadRequest("Current user is not impersonating.");
        }

        if (result.Status is StopImpersonationStatus.OriginalAdminNotFound
            or StopImpersonationStatus.OriginalAdminInactive)
        {
            httpResponse.DeleteAuthCookie();

            return TypedResults.Unauthorized();
        }

        return TypedResults.Problem("Stop impersonation failed.");
    }
}