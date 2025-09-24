using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.PDND.Exceptions;
using PandesalExpress.PDND.Services;
using Shared.Dtos;
using Shared.Events;
using System.Security.Claims;

namespace PandesalExpress.PDND.Features.UpdatePdndStatus;

public class UpdatePdndStatusHandler(
    AppDbContext context,
    IPdndStatusValidator statusValidator,
    IEventBus eventBus,
    ILogger<UpdatePdndStatusHandler> logger
) : ICommandHandler<UpdatePdndStatusCommand, PdndStatusUpdateResponseDto>
{
    public async Task<PdndStatusUpdateResponseDto> Handle(UpdatePdndStatusCommand command, CancellationToken cancellationToken)
    {
        var requestId = Ulid.Parse(command.RequestId);

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get the PDND request with related data
            PdndRequest? pdndRequest = await context.PdndRequests
                                                    .Include(p => p.Store)
                                                    .Include(p => p.RequestingEmployee)
                                                    .Include(p => p.PdndRequestItems)
                                                    .FirstOrDefaultAsync(p => p.Id == requestId, cancellationToken);

            if (pdndRequest == null)
            {
                throw new PdndRequestNotFoundException(command.RequestId);
            }

            var currentStatus = pdndRequest.Status;

            // Validate status transition
            if (!statusValidator.IsValidTransition(currentStatus, command.NewStatus))
            {
                throw new InvalidStatusTransitionException(currentStatus, command.NewStatus);
            }

            // Validate user permissions
            if (!statusValidator.CanUserUpdateStatus(command.User, currentStatus, command.NewStatus))
            {
                throw new UnauthorizedStatusUpdateException(command.NewStatus);
            }

            // Get user ID from claims
            Claim? userIdClaim = command.User.FindFirst("sub") ?? command.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }

            var userId = Ulid.Parse(userIdClaim.Value);

            // Update the request
            pdndRequest.Status = command.NewStatus;
            pdndRequest.StatusLastUpdated = DateTime.UtcNow;
            pdndRequest.LastUpdatedBy = userId;

            // Update notes if provided
            if (!string.IsNullOrEmpty(command.Notes))
            {
                pdndRequest.CommissaryNotes = command.Notes;
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Get username for response and event
            Employee? updatingUser = await context.Users.FindAsync([userId], cancellationToken: cancellationToken);
            var updatedByName = updatingUser?.FirstName ?? "Unknown User";

            // Create response DTO
            var response = new PdndStatusUpdateResponseDto
            {
                RequestId = command.RequestId,
                PreviousStatus = currentStatus,
                NewStatus = command.NewStatus,
                UpdatedAt = pdndRequest.StatusLastUpdated.Value,
                UpdatedBy = updatedByName,
                Notes = command.Notes
            };

            // Create PdndRequestDto for event
            var pdndRequestDto = new PdndRequestDto
            {
                Id = pdndRequest.Id.ToString(),
                StoreId = pdndRequest.StoreId.ToString(),
                RequestingEmployeeId = pdndRequest.RequestingEmployeeId.ToString(),
                CommissaryId = pdndRequest.CommissaryId?.ToString(),
                RequestDate = pdndRequest.RequestDate,
                DateNeeded = pdndRequest.DateNeeded,
                Status = pdndRequest.Status,
                CommissaryNotes = pdndRequest.CommissaryNotes,
                PdndRequestItems = [.. pdndRequest.PdndRequestItems.Select(item => new PdndRequestItemDto
                {
                    Id = item.Id.ToString(),
                    PdndRequestId = item.PdndRequestId.ToString(),
                    ProductId = item.ProductId.ToString(),
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    TotalAmount = item.TotalAmount
                })]
			};

            // Publish status changed event
            var statusChangedEvent = new PdndStatusChangedEvent(
                pdndRequestDto,
                currentStatus,
                command.NewStatus,
                updatedByName,
                command.Notes,
                pdndRequest.StatusLastUpdated.Value
            );

            await eventBus.PublishAsync(statusChangedEvent, cancellationToken);

            logger.LogInformation(
                "PDND request {RequestId} status updated from {PreviousStatus} to {NewStatus} by {UpdatedBy}",
                command.RequestId, currentStatus, command.NewStatus, updatedByName
            );

            return response;
        }
        catch (DbUpdateConcurrencyException e)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(e, "Concurrency conflict while updating PDND request status for {RequestId}", command.RequestId);
            throw new DbUpdateConcurrencyException("Concurrency conflict while updating PDND request status");
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(e, "Error updating PDND request status for {RequestId}", command.RequestId);
            throw;
        }
    }
}