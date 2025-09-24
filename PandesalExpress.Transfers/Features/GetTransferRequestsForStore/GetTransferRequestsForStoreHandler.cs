using Microsoft.EntityFrameworkCore;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Services;
using Shared.Dtos;

namespace PandesalExpress.Transfers.Features.GetTransferRequestsForStore;

public class GetTransferRequestsForStoreHandler(
	AppDbContext context,
	ICacheService cacheService
) : IQueryHandler<GetTransferRequestsForStoreQuery, List<TransferRequestDto>>
{
	public async Task<List<TransferRequestDto>> Handle(GetTransferRequestsForStoreQuery request, CancellationToken cancellationToken)
	{
		// Get the cache of TransferRequest for both sending and receiving store
		string cacheKey = $"store:transfer-requests:{request.StoreId}";
		var transferRequests = await cacheService.GetOrSetAsync(
			cacheKey,
			() => TransferRequestsAsync(request.StoreId, cancellationToken),
			TimeSpan.FromMinutes(10)
		);

		return [.. transferRequests!.Select(transferRequest => new TransferRequestDto
			{
				Id = transferRequest.Id.ToString(),
				SendingStoreId = transferRequest.SendingStoreId.ToString(),
				ReceivingStoreId = transferRequest.ReceivingStoreId.ToString(),
				InitiatingEmployeeId = transferRequest.InitiatingEmployeeId.ToString(),
				RespondingEmployeeId = transferRequest.RespondingEmployeeId.ToString(),
				Status = transferRequest.Status.ToString(),
				RequestNotes = transferRequest.RequestNotes,
				ResponseNotes = transferRequest.ResponseNotes,
				ShippedAt = transferRequest.ShippedAt,
				ReceivedAt = transferRequest.ReceivedAt,
				Items = [.. transferRequest.Items.Select(i => new TransferRequestItemDto
					{
						Id = i.Id.ToString(),
						ProductId = i.ProductId.ToString(),
						ProductName = i.ProductName,
						QuantityRequested = i.QuantityRequested
					}
				)]
		}
		)];
	}

	private async Task<List<TransferRequest>> TransferRequestsAsync(
		Ulid storeId,
		CancellationToken cancellationToken
	) => await context.TransferRequests
		.Include(tr => tr.Items)
		.Where(tr => tr.SendingStoreId == storeId || tr.ReceivingStoreId == storeId)
		.ToListAsync(cancellationToken);
}
