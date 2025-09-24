using PandesalExpress.Infrastructure.Models;
using System.Security.Claims;

namespace PandesalExpress.Transfers.Services;

public interface ITransferStatusValidator
{
    /// <summary>
    /// Validates if the transition from the current status to the new status is allowed
    /// </summary>
    /// <param name="currentStatus">The current status of the transfer request</param>
    /// <param name="newStatus">The new status to transition to</param>
    /// <returns>True if the transition is valid, false otherwise</returns>
    bool IsValidTransition(TransferStatus currentStatus, TransferStatus newStatus);
    
    /// <summary>
    /// Validates if the user has permission to update the status from current to new
    /// </summary>
    /// <param name="user">The user attempting to update the status</param>
    /// <param name="currentStatus">The current status of the transfer request</param>
    /// <param name="newStatus">The new status to transition to</param>
    /// <returns>True if the user has permission, false otherwise</returns>
    bool CanUserUpdateStatus(ClaimsPrincipal user, TransferStatus currentStatus, TransferStatus newStatus);
}