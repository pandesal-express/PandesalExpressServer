using System.ComponentModel.DataAnnotations;

namespace PandesalExpress.Transfers.Dtos;

public class AddTransferMessageDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 1000 characters")]
    public string Message { get; set; } = string.Empty;
}