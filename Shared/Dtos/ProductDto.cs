namespace Shared.Dtos;

public class ProductDto
{
    public required string Id { get; set; }
    public required string Category { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required string Shift { get; set; }
    public required int Quantity { get; set; }
    public string? Description { get; set; }

    public List<StoreInventoryDto>? StoreInventories { get; set; }
}
