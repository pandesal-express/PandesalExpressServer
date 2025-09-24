namespace Shared.Dtos;

public record AddStocksToStoreResponseDto
{
    public required string StoreId { get; init; }
    public required string StoreName { get; init; }
    public DateTime DeliveryDate { get; init; }
    public string? VerifiedByCommissaryName { get; init; }
    public string Message { get; init; } = "Stocks updated successfully.";
    public int ItemsProcessedCount { get; init; }
}
