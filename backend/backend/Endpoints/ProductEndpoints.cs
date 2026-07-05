using backend.DTOs.Products;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace backend.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization();

        group.MapGet("/my", GetMyProductsAsync);

        group.MapPut("/{productId:int}/favorite", SetFavoriteAsync);

        return app;
    }

    private static async Task<Ok<List<ProductDto>>> GetMyProductsAsync(
        ProductService productService)
    {
        var products = await productService.GetMyProductsAsync();

        return TypedResults.Ok(products);
    }

    private static async Task<Results<NoContent, NotFound<string>>> SetFavoriteAsync(
        int productId,
        SetFavoriteRequest request,
        ProductService productService)
    {

        var updated = await productService.SetFavoriteAsync(
            productId,
            request.IsFavorite);

        if (!updated)
        {
            return TypedResults.NotFound("Product was not found.");
        }

        return TypedResults.NoContent();
    }
}