using System.Security.Claims;
using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.PDND.Features.UpdatePdndStatus;

public record UpdatePdndStatusCommand(
    string RequestId,
    string NewStatus,
    string? Notes,
    ClaimsPrincipal User
) : ICommand<PdndStatusUpdateResponseDto>;