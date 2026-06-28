using backend.DTOs.Products;
using backend.Extensions;
using backend.Services;

namespace backend.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization();

        group.MapGet("/my", async (
            HttpContext httpContext,
            ProductService productService) =>
        {
            var userId = httpContext.User.GetUserId();
            var products = await productService.GetProductsForUserAsync(userId);

            return Results.Ok(products);
        });

        group.MapPut("/{productId:int}/favorite", async (
            int productId,
            SetFavoriteRequest request,
            HttpContext httpContext,
            ProductService productService) =>
        {
            var userId = httpContext.User.GetUserId();

            var updated = await productService.SetFavoriteAsync(
                userId,
                productId,
                request.IsFavorite);

            if (!updated)
            {
                return Results.NotFound("Product was not found.");
            }

            return Results.NoContent();
        });

        return app;
    }
}