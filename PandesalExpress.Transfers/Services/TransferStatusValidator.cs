using PandesalExpress.Infrastructure.Models;
using System.Security.Claims;

namespace PandesalExpress.Transfers.Services;

public class TransferStatusValidator : ITransferStatusValidator
{
    private static readonly Dictionary<TransferStatus, List<TransferStatus>> ValidTransitions = new()
    {
        { TransferStatus.Requested, new List<TransferStatus> { TransferStatus.Accepted, TransferStatus.Rejected, TransferStatus.Cancelled } },
        { TransferStatus.Accepted, new List<TransferStatus> { TransferStatus.Shipped, TransferStatus.Cancelled } },
        { TransferStatus.Rejected, new List<TransferStatus>() }, // Terminal state, no further transitions
        { TransferStatus.Shipped, new List<TransferStatus> { TransferStatus.Received } },
        { TransferStatus.Received, new List<TransferStatus>() }, // Terminal state, no further transitions
        { TransferStatus.Cancelled, new List<TransferStatus>() }  // Terminal state, no further transitions
    };

    /// <inheritdoc />
    public bool IsValidTransition(TransferStatus currentStatus, TransferStatus newStatus)
    {
        // If the status is not changing, it's always valid
        if (currentStatus == newStatus)
        {
            return true;
        }

        // Check if the transition is valid based on the defined rules
        return ValidTransitions.ContainsKey(currentStatus) && 
               ValidTransitions[currentStatus].Contains(newStatus);
    }

    /// <inheritdoc />
    public bool CanUserUpdateStatus(ClaimsPrincipal user, TransferStatus currentStatus, TransferStatus newStatus)
    {
        // First check if the transition itself is valid
        if (!IsValidTransition(currentStatus, newStatus))
        {
            return false;
        }

        // Get user roles
        var isStoreManager = user.IsInRole("StoreManager");
        var isRegionalManager = user.IsInRole("RegionalManager");
        
        // Define role-based permissions for status transitions
        switch (newStatus)
        {
            case TransferStatus.Accepted:
            case TransferStatus.Rejected:
                // Only store managers or regional managers can accept/reject requests
                return isStoreManager || isRegionalManager;
                
            case TransferStatus.Shipped:
                // Only store managers or regional managers can mark as shipped
                return isStoreManager || isRegionalManager;
                
            case TransferStatus.Received:
                // Only store managers or regional managers can mark as received
                return isStoreManager || isRegionalManager;
                
            case TransferStatus.Cancelled:
                // Anyone with appropriate roles can cancel (if in a cancellable state)
                return isStoreManager || isRegionalManager;
                
            default:
                return false;
        }
    }
}