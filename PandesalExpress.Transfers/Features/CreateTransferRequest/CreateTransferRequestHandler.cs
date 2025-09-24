using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Dtos;
using Shared.Events;

namespace PandesalExpress.Transfers.Features.CreateTransferRequest;

public class CreateTransferRequestHandler(
    AppDbContext context,
    IEventBus eventBus
) : ICommandHandler<CreateTransferRequestCommand, TransferRequestDto>
{
    public async Task<TransferRequestDto> Handle(CreateTransferRequestCommand request, CancellationToken cancellationToken)
    {
        var transferRequest = new TransferRequest
        {
            SendingStoreId = request.CreateTransferRequestDto.SendingStoreId,
            ReceivingStoreId = request.CreateTransferRequestDto.ReceivingStoreId,
            InitiatingEmployeeId = request.InitiatingEmployeeId,
            RequestNotes = request.CreateTransferRequestDto.RequestNotes,
            Status = TransferStatus.Requested,
            Items = [.. request.CreateTransferRequestDto.Items.Select(i => new TransferRequestItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    QuantityRequested = i.Quantity
                }
            )]
		};

        var transferRequestDto = new TransferRequestDto
        {
            Id = transferRequest.Id.ToString(),
            SendingStoreId = transferRequest.SendingStoreId.ToString(),
            ReceivingStoreId = transferRequest.ReceivingStoreId.ToString(),
            InitiatingEmployeeId = transferRequest.InitiatingEmployeeId.ToString(),
            Status = transferRequest.Status.ToString(),
            RequestNotes = transferRequest.RequestNotes,
            Items = [.. transferRequest.Items.Select(i => new TransferRequestItemDto
                {
                    Id = i.Id.ToString(),
                    ProductId = i.ProductId.ToString(),
                    ProductName = i.ProductName,
                    QuantityRequested = i.QuantityRequested
                }
            )]
		};

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await context.TransferRequests.AddAsync(transferRequest, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await eventBus.PublishAsync(new TransferRequestCreatedEvent(transferRequestDto), cancellationToken);
        }
        catch (DbUpdateConcurrencyException e)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new DbUpdateConcurrencyException("Concurrency conflict while creating transfer request", e);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return transferRequestDto;
    }
}
