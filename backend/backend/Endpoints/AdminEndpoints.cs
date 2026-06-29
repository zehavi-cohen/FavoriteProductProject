using backend.Extensions;
using backend.Services;

namespace backend.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/users", async (
            AdminService adminService) =>
        {
            var result = await adminService.GetUsersAsync();

            return ToHttpResult(result);
        });

        group.MapPost("/users/{userId:int}/login-as", async (
            int userId,
            HttpContext httpContext,
            AdminService adminService) =>
        {
            var adminUserId = httpContext.User.GetUserId();

            var result = await adminService.LoginAsUserAsync(
                userId,
                adminUserId);

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