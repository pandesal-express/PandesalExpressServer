namespace Shared.Dtos;

public class PdndRequestDto
{
    public string? Id { get; set; }
    public string? StoreId { get; set; }
    public string? RequestingEmployeeId { get; set; }
    public string? CommissaryId { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime DateNeeded { get; set; }
    public string? Status { get; set; }
    public string? CommissaryNotes { get; set; }
    public List<PdndRequestItemDto> PdndRequestItems { get; set; } = [];
}