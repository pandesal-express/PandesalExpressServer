using Microsoft.EntityFrameworkCore;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Transfers.Exceptions;
using Shared.Dtos;

namespace PandesalExpress.Transfers.Features.GetTransferRequest;

public class GetTransferRequestHandler(
    AppDbContext context
) : IQueryHandler<GetTransferRequestQuery, TransferRequestDto>
{
    public async Task<TransferRequestDto> Handle(GetTransferRequestQuery request, CancellationToken cancellationToken)
    {
        TransferRequest? transferRequest = await context.TransferRequests
                                                        .Include(tr => tr.Items)
                                                        .FirstOrDefaultAsync(tr => tr.Id == request.TransferRequestId, cancellationToken);

        if (transferRequest == null) throw new TransferRequestNotFoundException(request.TransferRequestId.ToString());

        return new TransferRequestDto
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
		};
    }
}
