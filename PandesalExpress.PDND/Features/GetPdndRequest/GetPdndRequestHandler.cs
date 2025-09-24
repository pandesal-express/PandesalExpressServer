using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.PDND.Exceptions;
using Shared.Dtos;
using System.Security.Claims;

namespace PandesalExpress.PDND.Features.GetPdndRequest;

public class GetPdndRequestHandler(
	AppDbContext context,
	ILogger<GetPdndRequestHandler> logger
) : IQueryHandler<GetPdndRequestQuery, PdndRequestDto>
{
	public async Task<PdndRequestDto> Handle(GetPdndRequestQuery query, CancellationToken cancellationToken)
	{
		try
		{
			if (!Ulid.TryParse(query.RequestId, out var requestId))
			{
				throw new ArgumentException($"Invalid request ID format: {query.RequestId}");
			}

			var pdndRequest = await context.PdndRequests
				.Include(p => p.Store)
				.Include(p => p.RequestingEmployee)
				.Include(p => p.PdndRequestItems)
				.FirstOrDefaultAsync(p => p.Id == requestId, cancellationToken)
					?? throw new PdndRequestNotFoundException(query.RequestId);

			// Apply role-based access control
			var userRoles = query.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
			var userId = query.User.FindFirst("sub")?.Value ?? query.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			// Store Operations can only see requests for their store
			if (userRoles.Contains("Store Operations")
				&& !userRoles.Contains("Stocks and Inventory")
				&& !string.IsNullOrEmpty(userId)
				&& Ulid.TryParse(userId, out var userUlid)
			)
			{
				var userStoreId = await context.Users
					.AsNoTracking()
					.Where(u => u.Id == userUlid)
					.Select(u => u.StoreId)
					.FirstOrDefaultAsync(cancellationToken);
				if (userStoreId.HasValue && pdndRequest.StoreId != userStoreId.Value)
				{
					throw new UnauthorizedAccessException("Access denied to this PDND request");
				}
			}

			var result = new PdndRequestDto
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

			logger.LogInformation(
				"Retrieved PDND request {RequestId} for user with roles: {Roles}",
				query.RequestId, string.Join(", ", userRoles)
			);

			return result;
		}
		catch (Exception e)
		{
			logger.LogError(e, "Error retrieving PDND request {RequestId}", query.RequestId);
			throw;
		}
	}
}
