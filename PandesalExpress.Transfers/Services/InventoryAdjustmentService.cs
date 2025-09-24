using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;

namespace PandesalExpress.Transfers.Services;

public class InventoryAdjustmentService(
    AppDbContext context,
    ILogger<InventoryAdjustmentService> logger
) : IInventoryAdjustmentService
{
    public async Task AdjustInventoryForTransferAsync(TransferRequest transferRequest, CancellationToken cancellationToken)
    {
        if (transferRequest.Status != TransferStatus.Received)
        {
            logger.LogWarning(
                "Cannot adjust inventory for transfer {TransferId} with status {Status}. Status must be Received.",
                transferRequest.Id,
                transferRequest.Status
            );
            return;
        }

        logger.LogInformation(
            "Adjusting inventory for transfer {TransferId} from store {SendingStoreId} to store {ReceivingStoreId}",
            transferRequest.Id,
            transferRequest.SendingStoreId,
            transferRequest.ReceivingStoreId
        );

        var productIds = transferRequest.Items.Select(item => item.ProductId).ToList();
        Ulid[] storeIds = [transferRequest.SendingStoreId, transferRequest.ReceivingStoreId];
        List<StoreInventory> inventories = await context.StoreInventories
                                                        .Where(si => storeIds.Contains(si.StoreId) && productIds.Contains(si.ProductId))
                                                        .ToListAsync(cancellationToken);
        var newInventories = new List<StoreInventory>();
        var updatedInventories = new List<StoreInventory>();

        foreach (TransferRequestItem item in transferRequest.Items)
        {
            StoreInventory? sendingStoreInventory = inventories.FirstOrDefault(si =>
                si.StoreId == transferRequest.SendingStoreId && si.ProductId == item.ProductId
            );
            StoreInventory? receivingStoreInventory = inventories.FirstOrDefault(si =>
                si.StoreId == transferRequest.ReceivingStoreId && si.ProductId == item.ProductId
            );

            if (sendingStoreInventory != null)
            {
                sendingStoreInventory.Quantity -= item.QuantityRequested;
                updatedInventories.Add(sendingStoreInventory);
            }

            if (receivingStoreInventory != null)
            {
                receivingStoreInventory.Quantity += item.QuantityRequested;
                updatedInventories.Add(receivingStoreInventory);
            }
            else
            {
                newInventories.Add(
                    new StoreInventory
                    {
                        Id = Ulid.NewUlid(),
                        StoreId = transferRequest.ReceivingStoreId,
                        ProductId = item.ProductId,
                        Quantity = item.QuantityRequested,
                        Price = sendingStoreInventory!.Price
                    }
                );
            }
        }

        if (updatedInventories.Count > 0) await context.BulkUpdateAsync(updatedInventories, cancellationToken: cancellationToken);

        if (newInventories.Count > 0) await context.BulkInsertAsync(newInventories, cancellationToken: cancellationToken);
    }
}
