using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Services;
using Shared.Dtos;
using Shared.Events;

namespace PandesalExpress.PDND.Features.CreatePdndRequest;

public class CreatePdndRequestHandler(
    AppDbContext context,
    INotificationService notificationService,
    ILogger<CreatePdndRequestHandler> logger
) : ICommandHandler<CreatePdndRequestCommand, PdndRequestDto>
{
    public async Task<PdndRequestDto> Handle(CreatePdndRequestCommand command, CancellationToken cancellationToken)
    {
        var storeId = Ulid.Parse(command.StoreId);
        var cashierId = Ulid.Parse(command.CashierId);

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var pdndRequest = new PdndRequest
            {
                Id = Ulid.NewUlid(),
                StoreId = storeId,
                RequestingEmployeeId = cashierId,
                RequestDate = DateTime.UtcNow,
                DateNeeded = command.DateNeeded,
                Status = "Pending"
            };
            var pdndRequestItems = command.Items.Select(product => new PdndRequestItem
                                              {
                                                  Id = Ulid.NewUlid(),
                                                  PdndRequestId = pdndRequest.Id,
                                                  ProductId = product.Id,
                                                  ProductName = product.Name,
                                                  Quantity = product.Quantity,
                                                  TotalAmount = product.Price * product.Quantity
                                              }
                                          ).ToList();

            await context.PdndRequests.AddAsync(pdndRequest, cancellationToken);
            await context.PdndRequestItems.AddRangeAsync(pdndRequestItems, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            string cashierName = (await context.Users.FindAsync([cashierId], cancellationToken: cancellationToken))!.FirstName;

            var pdndRequestDto = new PdndRequestDto
            {
                Id = pdndRequest.Id.ToString(),
                StoreId = storeId.ToString(),
                RequestingEmployeeId = cashierId.ToString(),
                RequestDate = pdndRequest.RequestDate,
                DateNeeded = pdndRequest.DateNeeded,
                Status = pdndRequest.Status,
                PdndRequestItems = [.. pdndRequestItems.Select(item => new PdndRequestItemDto
                    {
                        Id = item.Id.ToString(),
                        PdndRequestId = item.PdndRequestId.ToString(),
                        ProductId = item.ProductId.ToString(),
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        TotalAmount = item.TotalAmount
                    }
                )]
			};

            await notificationService.SendNotificationToGroupAsync(
                "Commissary",
                "NewPdndRequest",
                new NotificationDto(
                    Guid.NewGuid(),
                    $"New PDND Request from Store Branch code {command.StoreKey}",
                    "NewPdndRequest",
                    $"/commissary/pdnd/{pdndRequest.Id}",
                    DateTime.UtcNow,
                    false,
                    cashierName
                )
            );

            return pdndRequestDto;
        }
        catch (DbUpdateConcurrencyException e)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(e, "Concurrency conflict while creating PDND request.");

            throw new DbUpdateConcurrencyException("Concurrency conflict while creating PDND request");
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(e, "Error creating PDND request.");
            throw;
        }
    }
}
