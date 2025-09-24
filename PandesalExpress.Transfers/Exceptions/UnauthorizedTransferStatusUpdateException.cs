using PandesalExpress.Infrastructure.Models;

namespace PandesalExpress.Transfers.Exceptions;

public class UnauthorizedTransferStatusUpdateException : UnauthorizedAccessException
{
    public TransferStatus RequestedStatus { get; }

    public UnauthorizedTransferStatusUpdateException(TransferStatus requestedStatus)
        : base($"User is not authorized to update transfer status to {requestedStatus}")
    {
        RequestedStatus = requestedStatus;
    }
}