using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Transfers.Exceptions;
using PandesalExpress.Transfers.Services;
using Shared.Dtos;
using Shared.Events;

namespace PandesalExpress.Transfers.Features.UpdateTransferRequestStatus;

public class UpdateTransferRequestStatusHandler(
    AppDbContext context,
    ITransferStatusValidator statusValidator,
    IInventoryAdjustmentService inventoryAdjustmentService,
    IEventBus eventBus,
    ILogger<UpdateTransferRequestStatusHandler> logger
) : ICommandHandler<UpdateTransferRequestStatusCommand, TransferRequestDto>
{
    public async Task<TransferRequestDto> Handle(UpdateTransferRequestStatusCommand request, CancellationToken cancellationToken)
    {
        TransferRequest? transferRequest = await context.TransferRequests
                                                        .Include(tr => tr.Items)
                                                        .FirstOrDefaultAsync(tr => tr.Id == request.TransferRequestId, cancellationToken);

        if (transferRequest == null)
			throw new TransferRequestNotFoundException("Transfer request not found.");

        TransferStatus currentStatus = transferRequest.Status;
        TransferStatus newStatus = request.UpdateTransferStatusDto.Status;

        // Validate status transition
        if (!statusValidator.IsValidTransition(currentStatus, newStatus))
        {
            logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus}", currentStatus, newStatus);
            throw new InvalidTransferStatusTransitionException(currentStatus, newStatus);
        }

        // Validate user permissions
        if (!statusValidator.CanUserUpdateStatus(request.User, currentStatus, newStatus))
        {
            logger.LogWarning("User not authorized to update status from {CurrentStatus} to {NewStatus}", currentStatus, newStatus);
            throw new UnauthorizedTransferStatusUpdateException(newStatus);
        }

        string? notes = request.UpdateTransferStatusDto.ResponseNotes;
        string statusChangeMessage = newStatus switch
        {
            TransferStatus.Accepted => "Transfer request has been accepted.",
            TransferStatus.Rejected => $"Transfer request has been rejected. Reason: {notes ?? "No reason provided"}",
            TransferStatus.Shipped => "Items have been shipped.",
            TransferStatus.Received => "Items have been received.",
            TransferStatus.Cancelled => $"Transfer request has been cancelled. {(string.IsNullOrEmpty(notes) ? "" : $"Reason: {notes}")}",
            var _ => $"Transfer status changed from {currentStatus} to {newStatus}."
        };

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Update the transfer request
            transferRequest.Status = newStatus;
            transferRequest.ResponseNotes = request.UpdateTransferStatusDto.ResponseNotes;
            transferRequest.RespondingEmployeeId = request.RespondingEmployeeId;
            transferRequest.SystemMessage = statusChangeMessage;

            switch (newStatus)
            {
                case TransferStatus.Shipped:
                    transferRequest.ShippedAt = DateTime.UtcNow;
                    break;
                case TransferStatus.Received:
                    transferRequest.ReceivedAt = DateTime.UtcNow;
                    // Adjust inventory quantities for both stores
                    await inventoryAdjustmentService.AdjustInventoryForTransferAsync(transferRequest, cancellationToken);
                    break;
                case TransferStatus.Requested:
                case TransferStatus.Accepted:
                case TransferStatus.Rejected:
                case TransferStatus.Cancelled:
                    break;
                default:
                    throw new TransferStatusOutOfRangeException(newStatus);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var transferRequestDto = new TransferRequestDto
            {
                Id = transferRequest.Id.ToString(),
                SendingStoreId = transferRequest.SendingStoreId.ToString(),
                ReceivingStoreId = transferRequest.ReceivingStoreId.ToString(),
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

            await eventBus.PublishAsync(new TransferRequestStatusUpdatedEvent(transferRequestDto), cancellationToken);

            return transferRequestDto;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError("Concurrency conflict while updating transfer request status");
            throw new DbUpdateConcurrencyException("Concurrency conflict while updating transfer request status");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError("Error updating transfer request status");
            throw;
        }
    }
}
