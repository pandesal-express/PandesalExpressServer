namespace Shared.Dtos;

public class TransferRequestDto
{
    public required string Id { get; init; }
    public string? SendingStoreId { get; init; }
    public string? ReceivingStoreId { get; init; }
    public string? InitiatingEmployeeId { get; init; }
    public string? RespondingEmployeeId { get; init; }
    public required string Status { get; set; }
    public string? RequestNotes { get; set; }
    public string? ResponseNotes { get; set; }
    public string? SystemMessage { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public List<TransferRequestItemDto> Items { get; set; } = [];
}

public class TransferRequestItemDto
{
    public required string Id { get; init; }
    public required string ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int QuantityRequested { get; init; }
}
