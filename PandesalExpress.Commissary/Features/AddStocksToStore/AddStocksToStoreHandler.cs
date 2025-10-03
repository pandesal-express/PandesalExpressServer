using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using Shared.Dtos;

namespace PandesalExpress.Commissary.Features.AddStocksToStore;

public class AddStocksToStoreHandler(
    AppDbContext context,
    ILogger<AddStocksToStoreHandler> logger
) : ICommandHandler<AddStocksToStoreCommand, AddStocksToStoreResponseDto>
{
    public async Task<AddStocksToStoreResponseDto> Handle(AddStocksToStoreCommand request, CancellationToken cancellationToken)
    {
        var storeId = Ulid.Parse(request.StoreId);
        Store? store = await context.Stores.FindAsync([storeId], cancellationToken);

        if (store is null)
            throw new KeyNotFoundException($"Store with ID {request.StoreId} not found");

        var productIds = request.RequestDto.DeliveredItems
                            .Select(item => Ulid.Parse(item.ProductId))
                            .ToList();

        Dictionary<Ulid, StoreInventory> existingStoreInventories = await context.StoreInventories
                            .Where(si => si.StoreId == storeId && productIds.Contains(si.ProductId))
                            .ToDictionaryAsync(si => si.ProductId, si => si, cancellationToken);
    
        var inventoriesToAdd = new List<StoreInventory>();
        DateTime now = DateTime.UtcNow;

        foreach (DeliverStockItemDto item in request.RequestDto.DeliveredItems)
        {
            var productId = Ulid.Parse(item.ProductId);

            if (existingStoreInventories.TryGetValue(productId, out StoreInventory? existingInventory))
            {
                existingInventory.Quantity += item.QuantityDelivered;
                existingInventory.Price = item.PriceInStore;
                existingInventory.LastVerified = now;
            }
            else
            {
                var newInventoryItem = new StoreInventory
                {
                    Id = Ulid.NewUlid(),
                    StoreId = storeId,
                    ProductId = productId,
                    Quantity = item.QuantityDelivered,
                    Price = item.PriceInStore,
                    LastVerified = now
                };

                inventoriesToAdd.Add(newInventoryItem);
            }
        }

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (inventoriesToAdd.Count != 0) await context.StoreInventories.AddRangeAsync(inventoriesToAdd, cancellationToken);

            store.StocksDateVerified = now;
            context.Stores.Update(store);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            string? commissaryName = request.User.FindFirstValue(ClaimTypes.Name);

            return new AddStocksToStoreResponseDto
            {
                StoreId = store.Id.ToString(),
                StoreName = store.Name,
                DeliveryDate = now,
                VerifiedByCommissaryName = commissaryName,
                ItemsProcessedCount = request.RequestDto.DeliveredItems.Count
            };
        }
        catch (DBConcurrencyException e)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(e, "Concurrency conflict while adding stock to store {StoreId}", request.StoreId);

            throw new DbUpdateConcurrencyException("Concurrency conflict while adding stock to store");
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(e, "Unhandled exception while adding stock to store {StoreId}", request.StoreId);

            throw;
        }
    }
}
