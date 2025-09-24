using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.Transfers.Features.GetTransferRequest;

public class GetTransferRequestQuery(Ulid transferRequestId) : IQuery<TransferRequestDto>
{
    public Ulid TransferRequestId { get; } = transferRequestId;
}
