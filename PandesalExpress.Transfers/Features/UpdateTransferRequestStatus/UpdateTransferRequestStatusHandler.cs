
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Services;
using PandesalExpress.Transfers.Exceptions;
using PandesalExpress.Transfers.Services;
using Shared.Dtos;
using Shared.Events;

namespace PandesalExpress.Transfers.Features.UpdateTransferRequestStatus;

public class UpdateTransferRequestStatusHandler(
    AppDbContext context,
    ITransferStatusValidator statusValidator,
    IEventBus eventBus,
    ICacheService cacheService,
    ILogger<UpdateTransferRequestStatusHandler> logger
) : ICommandHandler<UpdateTransferRequestStatusCommand, TransferRequestDto>
{
    public async Task<TransferRequestDto> Handle(UpdateTransferRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var cacheKey = $"transfer:{request.TransferRequestId}";

        // Contains:
        // status, sending_store_id, receiving_store_id, request_notes, response_notes
        Dictionary<string, string> transferCache = await cacheService.GetHashAsync(cacheKey);

        if (transferCache.Count == 0)
            throw new TransferRequestNotFoundException("Transfer request not found.");

        TransferStatus currentStatus = Enum.Parse<TransferStatus>(transferCache["status"]);
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

        string? responseNote = request.UpdateTransferStatusDto.ResponseNotes;
        string statusChangeMessage = newStatus switch
        {
            TransferStatus.Accepted => "Transfer request has been accepted.",
            TransferStatus.Rejected => $"Transfer request has been rejected. Reason: {responseNote ?? "No reason provided"}",
            TransferStatus.Shipped => "Items have been shipped.",
            TransferStatus.Received => "Items have been received.",
            TransferStatus.Cancelled => $"Transfer request has been cancelled. {(string.IsNullOrEmpty(responseNote) ? "" : $"Reason: {responseNote}")}",
            var _ => $"Transfer status changed from {currentStatus} to {newStatus}."
        };

        // Set the status, request notes, and response notes only to Redis
        await cacheService.SetHashFieldAsync(cacheKey, "status", newStatus.ToString());

        if (!string.IsNullOrEmpty(responseNote))
            await cacheService.SetHashFieldAsync(cacheKey, "response_notes", responseNote);

        if (!string.IsNullOrEmpty(request.UpdateTransferStatusDto.RequestNotes))
            await cacheService.SetHashFieldAsync(cacheKey, "request_notes", request.UpdateTransferStatusDto.RequestNotes);

        if (newStatus == TransferStatus.Shipped)
            await cacheService.SetHashFieldAsync(cacheKey, "shipped_at", DateTime.UtcNow.ToString("O"));

        TransferRequest? transferRequest = await context.TransferRequests
                                                        .Include(tr => tr.Items)
                                                        .FirstOrDefaultAsync(tr => tr.Id == request.TransferRequestId, cancellationToken);

        if (transferRequest == null)
            throw new TransferRequestNotFoundException("Transfer request not found.");

        var transferRequestDto = new TransferRequestDto
        {
            Id = request.TransferRequestId.ToString(),
            Status = newStatus.ToString(),
            RequestNotes = request.UpdateTransferStatusDto.RequestNotes,
            ResponseNotes = responseNote,
            ShippedAt = transferRequest.ShippedAt,
            ReceivedAt = transferRequest.ReceivedAt,
            SystemMessage = statusChangeMessage
        };

        if (newStatus == TransferStatus.Received)
        {
            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // When received, update the changed fields to DB as final step
                transferRequest.Status = newStatus;
                transferRequest.ResponseNotes = request.UpdateTransferStatusDto.ResponseNotes;
                transferRequest.RespondingEmployeeId = request.RespondingEmployeeId;
                transferRequest.SystemMessage = statusChangeMessage;
                transferRequest.ShippedAt = DateTime.UtcNow;
                transferRequest.ReceivedAt = DateTime.UtcNow;

                await AdjustInventoriesAsync(transferRequest, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                transferRequestDto.Items =
                [
                    .. transferRequest.Items.Select(i => new TransferRequestItemDto
                        {
                            Id = i.Id.ToString(),
                            ProductId = i.ProductId.ToString(),
                            ProductName = i.ProductName,
                            QuantityRequested = i.QuantityRequested
                        }
                    )
                ];
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

        await eventBus.PublishAsync(new TransferRequestStatusUpdatedEvent(transferRequestDto), cancellationToken);

        return transferRequestDto;
    }

    private async Task AdjustInventoriesAsync(TransferRequest transferRequest, CancellationToken cancellationToken)
    {
        var productIds = transferRequest.Items.Select(item => item.ProductId).ToHashSet();
        var storeIds = new HashSet<Ulid>
        {
            transferRequest.SendingStoreId,
            transferRequest.ReceivingStoreId
        };

        var inventories = await context.StoreInventories
                                       .Where(si => storeIds.Contains(si.StoreId) && productIds.Contains(si.ProductId))
                                       .Select(si => new
                                           {
                                               si.Id,
                                               si.StoreId,
                                               si.ProductId,
                                               si.Price,
                                               si.RowVersion,
                                               Entity = new StoreInventory
                                               {
                                                   Id = si.Id,
                                                   Quantity = si.Quantity,
                                                   Price = si.Price
                                               }
                                           }
                                       ).ToListAsync(cancellationToken);

        var inventoryDict = inventories.ToDictionary(
            i => new
            {
                i.StoreId,
                i.ProductId
            },
            i => i
        );

        var inventoriesToUpdate = new List<StoreInventory>();
        var inventoriesToInsert = new List<StoreInventory>();
        var inventoriesToDelete = new List<StoreInventory>();

        foreach (TransferRequestItem item in transferRequest.Items)
        {
            var sendingId = new
            {
                StoreId = transferRequest.SendingStoreId,
                item.ProductId
            };
            var receivingId = new
            {
                StoreId = transferRequest.ReceivingStoreId,
                item.ProductId
            };

            if (!inventoryDict.TryGetValue(sendingId, out var sendingInventory))
                throw new InvalidOperationException(
                    $"No inventory record found for product {item.ProductName} in sending store"
                );

            // Update sending store inventory
            sendingInventory.Entity.Quantity -= item.QuantityRequested;
            sendingInventory.Entity.UpdatedAt = DateTime.UtcNow;

            // Check if the quantity of product is 0, remove that product to store inventory
            if (sendingInventory.Entity.Quantity == 0)
                inventoriesToDelete.Add(sendingInventory.Entity);
            else
                inventoriesToUpdate.Add(sendingInventory.Entity);

            if (inventoryDict.TryGetValue(receivingId, out var receivingInventory))
            {
                // Update receiving store inventory
                receivingInventory.Entity.Quantity += item.QuantityRequested;
                receivingInventory.Entity.UpdatedAt = DateTime.UtcNow;
                inventoriesToUpdate.Add(receivingInventory.Entity);
            }
            else
            {
                var newInventory = new StoreInventory
                {
                    Id = Ulid.NewUlid(),
                    StoreId = transferRequest.ReceivingStoreId,
                    ProductId = item.ProductId,
                    Quantity = item.QuantityRequested,
                    Price = sendingInventory.Price
                };
                inventoriesToInsert.Add(newInventory);
            }
        }

        // Concurrency token handling config
        var bulkConfig = new BulkConfig
        {
            BatchSize = 50, // The number of products are just mostly between 5-15, who would transfer 20+ diff kind of products?
            BulkCopyTimeout = 30,
            EnableStreaming = true,
            UpdateByProperties = [nameof(StoreInventory.Id)],
            PropertiesToIncludeOnUpdate =
            [
                nameof(StoreInventory.Quantity),
                nameof(StoreInventory.UpdatedAt)
            ],
            // Optimistic concurrency control
            DoNotUpdateIfTimeStampChanged = true
        };

        if (inventoriesToUpdate.Count > 0)
            await context.BulkUpdateAsync(
                inventoriesToUpdate,
                bulkConfig,
                cancellationToken: cancellationToken
            );

        if (inventoriesToInsert.Count > 0)
            await context.BulkInsertAsync(
                inventoriesToInsert,
                bulkConfig,
                cancellationToken: cancellationToken
            );

        if (inventoriesToDelete.Count > 0)
            await context.BulkDeleteAsync(
                inventoriesToDelete,
                bulkConfig,
                cancellationToken: cancellationToken
            );
    }
}
