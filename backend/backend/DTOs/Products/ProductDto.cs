namespace backend.DTOs.Products;

public class ProductDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string? Description { get; set; }

    public bool IsFavorite { get; set; }
}