using PandesalExpress.Infrastructure.Models;

namespace PandesalExpress.Transfers.Exceptions;

public class InvalidTransferStatusTransitionException : Exception
{
    public TransferStatus CurrentStatus { get; }
    public TransferStatus NewStatus { get; }

    public InvalidTransferStatusTransitionException(TransferStatus currentStatus, TransferStatus newStatus)
        : base($"Invalid status transition from {currentStatus} to {newStatus}")
    {
        CurrentStatus = currentStatus;
        NewStatus = newStatus;
    }
}