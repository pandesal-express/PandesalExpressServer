using System.Security.Claims;

namespace PandesalExpress.PDND.Services;

public interface IPdndStatusValidator
{
    bool IsValidTransition(string currentStatus, string newStatus);
    bool CanUserUpdateStatus(ClaimsPrincipal user, string currentStatus, string newStatus);
    List<string> GetAllowedNextStatuses(string currentStatus, ClaimsPrincipal user);
    List<string> GetTargetRolesForStatus(string status);
}