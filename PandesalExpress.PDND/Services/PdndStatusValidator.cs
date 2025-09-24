using System.Security.Claims;

namespace PandesalExpress.PDND.Services;

public class PdndStatusValidator : IPdndStatusValidator
{
    private static readonly Dictionary<string, List<string>> ValidTransitions = new()
    {
        ["Requested"] = ["Delivered", "Cancelled"],
        ["Delivered"] = ["Verified", "Cancelled"],
        ["Verified"] = ["PickedUp", "Cancelled"],
        ["PickedUp"] = [],
        ["Cancelled"] = []
    };

    private static readonly Dictionary<string, List<string>> StatusRolePermissions = new()
    {
        ["Delivered"] = ["Stocks and Inventory"], // Only commissary can mark as delivered
        ["Verified"] = ["Store Operations"], // Only store staff can verify
        ["PickedUp"] = ["Store Operations"], // Only store staff can mark as picked up
        ["Cancelled"] = ["Store Operations", "Stocks and Inventory"] // Both can cancel
    };

    private static readonly Dictionary<string, List<string>> StatusNotificationTargets = new()
    {
        ["Requested"] = ["Stocks and Inventory"], // Notify commissary
        ["Delivered"] = ["Store Operations"], // Notify store
        ["Verified"] = ["Stocks and Inventory"], // Notify commissary
        ["PickedUp"] = ["Stocks and Inventory"], // Notify commissary
        ["Cancelled"] = ["Store Operations", "Stocks and Inventory"] // Notify both
    };

    public bool IsValidTransition(string currentStatus, string newStatus)
    {
        if (string.IsNullOrEmpty(currentStatus) || string.IsNullOrEmpty(newStatus))
            return false;

        return ValidTransitions.TryGetValue(currentStatus, out List<string>? allowedStatuses) &&
               allowedStatuses.Contains(newStatus);
    }

    public bool CanUserUpdateStatus(ClaimsPrincipal user, string currentStatus, string newStatus)
    {
        if (!IsValidTransition(currentStatus, newStatus))
            return false;

        // Get required roles for the new status
        if (!StatusRolePermissions.TryGetValue(newStatus, out var requiredRoles))
            return false;

        // Check if user has any of the required roles
        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        return requiredRoles.Any(role => userRoles.Contains(role));
    }

    public List<string> GetAllowedNextStatuses(string currentStatus, ClaimsPrincipal user)
    {
        if (!ValidTransitions.TryGetValue(currentStatus, out List<string>? possibleStatuses))
            return [];

        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return [.. possibleStatuses
            .Where(status => StatusRolePermissions.TryGetValue(status, out var requiredRoles) &&
                           requiredRoles.Any(role => userRoles.Contains(role)))];
    }

    public List<string> GetTargetRolesForStatus(string status) =>
        StatusNotificationTargets.TryGetValue(status, out var roles) ? roles : [];
}