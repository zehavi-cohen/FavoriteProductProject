namespace backend.DTOs.Products;

public sealed record ProductDto
(
     int ProductId,

     string ProductName,

     string? Code,

     string? Description,

     bool IsFavorite
);