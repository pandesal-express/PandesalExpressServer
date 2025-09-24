namespace Shared.Dtos;

public class TransferRequestDto
{
    public required string Id { get; init; }
    public required string SendingStoreId { get; init; }
    public required string ReceivingStoreId { get; init; }
    public string? InitiatingEmployeeId { get; init; }
    public string? RespondingEmployeeId { get; init; }
    public required string Status { get; init; }
    public string? RequestNotes { get; init; }
    public string? ResponseNotes { get; init; }
    public DateTime? ShippedAt { get; init; }
    public DateTime? ReceivedAt { get; init; }
    public List<TransferRequestItemDto> Items { get; init; } = [];
}

public class TransferRequestItemDto
{
    public required string Id { get; init; }
    public required string ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int QuantityRequested { get; init; }
}
