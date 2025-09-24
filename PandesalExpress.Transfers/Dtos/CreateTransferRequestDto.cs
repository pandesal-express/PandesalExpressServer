namespace PandesalExpress.Transfers.Dtos;

public class CreateTransferRequestDto
{
    public Ulid SendingStoreId { get; set; }
    public Ulid ReceivingStoreId { get; set; }
    public string? RequestNotes { get; set; }
    public required List<CreateTransferRequestItemDto> Items { get; set; } = [];
}

public class CreateTransferRequestItemDto
{
    public required Ulid ProductId { get; set; }
    public required string ProductName { get; set; }
    public int Quantity { get; set; }
}
