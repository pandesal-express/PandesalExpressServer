using PandesalExpress.Infrastructure.Models;

namespace PandesalExpress.Transfers.Dtos;

public class UpdateTransferStatusDto
{
    public TransferStatus Status { get; set; }
    public string? ResponseNotes { get; set; }
}
