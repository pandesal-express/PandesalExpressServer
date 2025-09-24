namespace Shared.Dtos;

public class PdndRequestItemDto
{
    public string? Id { get; set; }
    public string? PdndRequestId { get; set; }
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
}