using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PandesalExpress.Cashier.Exceptions;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using Shared.Dtos;

namespace PandesalExpress.Cashier.Features.LogSales;

public class LogSalesHandler(
    AppDbContext context,
    ILogger<LogSalesHandler> logger
) : IQueryHandler<LogSalesQuery, LogSalesResponseDto>
{
    public async Task<LogSalesResponseDto> Handle(LogSalesQuery request, CancellationToken cancellationToken)
    {
        string employeeId = request.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        string cashierName = request.User.FindFirstValue(ClaimTypes.Name)!;
        Ulid storeUlid = request.StoreUlid;

        var salesLogItemsToCreate = new List<SalesLogItem>();
        decimal transactionTotalAmount = 0;
        DateTime serverTransactionTime = DateTime.UtcNow;

        var productIds = request.RequestDto.Items
                                .Select(i => i.ProductId)
                                .Select(Ulid.Parse)
                                .ToList();

        Dictionary<Ulid, StoreInventory> storeInventoryItemsDict = await context.StoreInventories
                                                                                .Include(si => si.Product)
                                                                                .Where(si => si.StoreId == request.StoreUlid && productIds.Contains(si.ProductId))
                                                                                .ToDictionaryAsync(si => si.ProductId, si => si, cancellationToken);

        foreach (LeftOverProductDto purchasedItemDto in request.RequestDto.Items)
        {
            var productUlid = Ulid.Parse(purchasedItemDto.ProductId);

            if (!storeInventoryItemsDict.TryGetValue(productUlid, out StoreInventory? currentInventoryItem))
                throw new NotFoundException($"Product ID {purchasedItemDto.ProductId} not found in store's inventory list.");

            if (currentInventoryItem.Quantity < purchasedItemDto.Quantity)
                throw new ConflictException(
                    $"Insufficient stock for product '{currentInventoryItem.Product.Name}'. " +
                    $"Available: {currentInventoryItem.Quantity}, Requested: {purchasedItemDto.Quantity}."
                );

            currentInventoryItem.Quantity -= purchasedItemDto.Quantity;

            decimal itemAmount = currentInventoryItem.Price * purchasedItemDto.Quantity;
            salesLogItemsToCreate.Add(
                new SalesLogItem
                {
                    Id = Ulid.NewUlid(),
                    ProductId = productUlid,
                    Quantity = purchasedItemDto.Quantity,
                    PriceAtSale = currentInventoryItem.Price,
                    Amount = itemAmount
                }
            );
            transactionTotalAmount += itemAmount;
        }

        await using IDbContextTransaction dbTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var salesLog = new SalesLog
            {
                Id = Ulid.NewUlid(),
                StoreId = request.StoreUlid,
                EmployeeId = Ulid.Parse(employeeId),
                TotalPrice = transactionTotalAmount,
                Name = $"Sales by {cashierName} on {serverTransactionTime:yyyy-MM-dd}",
                Quantity = request.RequestDto.Items.Sum(i => i.Quantity),
                SalesLogItems = salesLogItemsToCreate,
                Shift = request.RequestDto.Shift
            };

            await context.SalesLogs.AddAsync(salesLog, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            return new LogSalesResponseDto
            {
                SalesLogId = salesLog.Id.ToString(),
                ServerTransactionTime = serverTransactionTime,
                ItemsProcessed = request.RequestDto.Items.Count,
                TotalAmount = transactionTotalAmount
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            context.ChangeTracker.Clear();
            logger.LogWarning(ex, "Concurrency conflict during sales logging for store {StoreId}.", storeUlid);
            throw new ConflictException(
                "A stock level changed while processing the transaction, or another data conflict occurred. Please try again."
            );
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error during adding sales log for store {StoreId}.", storeUlid);
            throw;
        }
    }
}
