using System.Security.Claims;
using PandesalExpress.Infrastructure.Models;

namespace PandesalExpress.Transfers.Services;

public class TransferStatusValidator : ITransferStatusValidator
{
    private static readonly Dictionary<TransferStatus, List<TransferStatus>> ValidTransitions = new()
    {
        {
            TransferStatus.Requested, [
                TransferStatus.Accepted, TransferStatus.Rejected,
                TransferStatus.Cancelled
            ]
        },
        { TransferStatus.Accepted, [TransferStatus.Shipped, TransferStatus.Cancelled] },
        { TransferStatus.Rejected, [] },
        { TransferStatus.Shipped, [TransferStatus.Received] },
        { TransferStatus.Received, [] },
        { TransferStatus.Cancelled, [] }
    };

    /// <inheritdoc />
    public bool IsValidTransition(TransferStatus currentStatus, TransferStatus newStatus)
    {
        if (currentStatus == newStatus) return true;

        // Check if the transition is valid based on the defined rules
        return ValidTransitions.ContainsKey(currentStatus) &&
               ValidTransitions[currentStatus].Contains(newStatus);
    }

    /// <inheritdoc />
    public bool CanUserUpdateStatus(
        ClaimsPrincipal user,
        TransferStatus currentStatus,
        TransferStatus newStatus
    ) => IsValidTransition(currentStatus, newStatus) && user.IsInRole("Store Manager");
}
