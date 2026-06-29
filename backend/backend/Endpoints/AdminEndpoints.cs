using backend.DTOs.Products;
using backend.Services;
using backend.Extensions;

namespace backend.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/users", GetUsersAsync);

        group.MapGet("/users/{userId:int}/products", GetUserProductsAsync);

        group.MapPut(
            "/users/{userId:int}/products/{productId:int}/favorite",
            SetFavoriteForUserAsync
        );

        return app;
    }

    private static async Task<IResult> GetUsersAsync(
        AdminService adminService)
    {
        var result = await adminService.GetUsersAsync();

        return result.ToHttpResult();
    }

    private static async Task<IResult> GetUserProductsAsync(
        int userId,
        AdminService adminService)
    {
        var result = await adminService.GetUserProductsAsync(userId);

        return result.ToHttpResult();
    }

    private static async Task<IResult> SetFavoriteForUserAsync(
        int userId,
        int productId,
        SetFavoriteRequest request,
        AdminService adminService)
    {
        var result = await adminService.SetFavoriteForUserAsync(
            userId,
            productId,
            request.IsFavorite);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.ToHttpResult();
    }

}