using System.ComponentModel.DataAnnotations;

namespace PandesalExpress.PDND.Dtos;

public record UpdatePdndStatusRequestDto
{
    [Required]
    [StringLength(20, ErrorMessage = "Status must not exceed 20 characters")]
    public required string NewStatus { get; init; }
    
    [StringLength(500, ErrorMessage = "Notes must not exceed 500 characters")]
    public string? Notes { get; init; }
}