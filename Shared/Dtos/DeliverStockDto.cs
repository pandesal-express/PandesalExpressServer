namespace Shared.Dtos;

public class DeliverStockItemDto
{
    public required string ProductId { get; set; } // Ulid
    public required int QuantityDelivered { get; set; }
    public decimal PriceInStore { get; set; } // Price set by commissary for this delivery
    public DateTime? PullOutDateTimeUtc { get; set; }
}

public class DeliverStocksRequestDto
{
    public required string Shift { get; set; } // "AM" or "PM"
    public required List<DeliverStockItemDto> DeliveredItems { get; set; }
    public string? DeliveredByCommissaryId { get; set; }
}
