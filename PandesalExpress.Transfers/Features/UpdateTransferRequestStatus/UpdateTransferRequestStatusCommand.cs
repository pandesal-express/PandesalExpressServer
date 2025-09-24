using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Transfers.Dtos;
using Shared.Dtos;
using System.Security.Claims;

namespace PandesalExpress.Transfers.Features.UpdateTransferRequestStatus;

public class UpdateTransferRequestStatusCommand(
    Ulid transferRequestId,
    UpdateTransferStatusDto updateTransferStatusDto,
    Ulid respondingEmployeeId,
    ClaimsPrincipal user
) : ICommand<TransferRequestDto>
{
    public Ulid TransferRequestId { get; } = transferRequestId;
    public UpdateTransferStatusDto UpdateTransferStatusDto { get; } = updateTransferStatusDto;
    public Ulid RespondingEmployeeId { get; } = respondingEmployeeId;
    public ClaimsPrincipal User { get; } = user;
}
