using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.Transfers.Features.GetTransferRequestsForStore;

public class GetTransferRequestsForStoreQuery(Ulid storeId) : IQuery<List<TransferRequestDto>>
{
    public Ulid StoreId { get; } = storeId;
}
