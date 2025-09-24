using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.PDND.Dtos;
using Shared.Dtos;
using System.Security.Claims;

namespace PandesalExpress.PDND.Features.GetPdndRequests;

public class GetPdndRequestsHandler(
    AppDbContext context,
    ILogger<GetPdndRequestsHandler> logger
) : IQueryHandler<GetPdndRequestsQuery, PdndRequestsResponseDto>
{
    public async Task<PdndRequestsResponseDto> Handle(GetPdndRequestsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var queryable = context.PdndRequests
                .Include(p => p.Store)
                .Include(p => p.RequestingEmployee)
                .Include(p => p.PdndRequestItems)
                .AsQueryable();

            // Apply role-based filtering
            var userRoles = query.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var userId = query.User.FindFirst("sub")?.Value ?? query.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Store Operations can only see requests for their store
            if (userRoles.Contains("Store Operations") && !userRoles.Contains("Stocks and Inventory"))
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    var userStoreId = await GetUserStoreId(userId, cancellationToken);
                    if (userStoreId.HasValue)
                    {
                        queryable = queryable.Where(p => p.StoreId == userStoreId.Value);
                    }
                }
            }

            // Apply filters
            if (!string.IsNullOrEmpty(query.StoreId) && Ulid.TryParse(query.StoreId, out var storeId))
            {
                queryable = queryable.Where(p => p.StoreId == storeId);
            }

            if (!string.IsNullOrEmpty(query.Status))
            {
                queryable = queryable.Where(p => p.Status == query.Status);
            }

            if (query.FromDate.HasValue)
            {
                queryable = queryable.Where(p => p.RequestDate >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                queryable = queryable.Where(p => p.RequestDate <= query.ToDate.Value);
            }

            // Apply sorting - priority by date needed, then request time
            queryable = queryable.OrderBy(p => p.DateNeeded).ThenBy(p => p.RequestDate);

            // Get total count before pagination
            var totalCount = await queryable.CountAsync(cancellationToken);

            // Apply pagination
            var requests = await queryable
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PdndRequestDto
                {
                    Id = p.Id.ToString(),
                    StoreId = p.StoreId.ToString(),
                    RequestingEmployeeId = p.RequestingEmployeeId.ToString(),
                    CommissaryId = p.CommissaryId != null ? p.CommissaryId.ToString() : null,
                    RequestDate = p.RequestDate,
                    DateNeeded = p.DateNeeded,
                    Status = p.Status,
                    CommissaryNotes = p.CommissaryNotes,
                    PdndRequestItems = p.PdndRequestItems.Select(item => new PdndRequestItemDto
                    {
                        Id = item.Id.ToString(),
                        PdndRequestId = item.PdndRequestId.ToString(),
                        ProductId = item.ProductId.ToString(),
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        TotalAmount = item.TotalAmount
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            var response = new PdndRequestsResponseDto
            {
                Requests = requests,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = totalPages,
                HasNextPage = query.Page < totalPages,
                HasPreviousPage = query.Page > 1
            };

            logger.LogInformation(
                "Retrieved {Count} PDND requests (page {Page} of {TotalPages}) for user with roles: {Roles}",
                requests.Count, query.Page, totalPages, string.Join(", ", userRoles)
            );

            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving PDND requests for query: {@Query}", query);
            throw;
        }
    }

    private async Task<Ulid?> GetUserStoreId(string userId, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(userId, out var userUlid))
            return null;

        var user = await context.Users
            .Where(u => u.Id == userUlid)
            .Select(u => u.StoreId)
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}