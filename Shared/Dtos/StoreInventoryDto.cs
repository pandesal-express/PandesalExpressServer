namespace Shared.Dtos;

public class StoreInventoryDto
{
    public string Id { get; set; }
    public string StoreId { get; set; }
    public string StoreKey { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? LastVerified { get; set; }
    public string ProductName { get; set; }
    public string ProductCategory { get; set; }

    public StoreDto Store { get; set; }
    public ProductDto Product { get; set; }
}
