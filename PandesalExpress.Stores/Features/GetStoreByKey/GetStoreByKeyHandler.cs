using Microsoft.EntityFrameworkCore;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Services;
using Shared.Dtos;

namespace PandesalExpress.Stores.Features.GetStoreByKey;

public class GetStoreByKeyQueryHandler(AppDbContext context, ICacheService cacheService) : IQueryHandler<GetStoreByKeyQuery, StoreDto?>
{
    public async Task<StoreDto?> Handle(GetStoreByKeyQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"store:key:{request.StoreKey}";

        StoreDto? storeDto = await cacheService.GetOrSetAsync(cacheKey, (Func<Task<StoreDto?>>)StoreFromDbFactory, TimeSpan.FromHours(1));

        if (storeDto is null) return null;

        // --- Determine Current and Previous Shift Logic ---
        DateTime nowUtc = DateTime.UtcNow;
        TimeSpan currentTimeOfDay = nowUtc.TimeOfDay;
        DateTime currentShiftStartDateThreshold;

        var amShiftStart = new TimeSpan(5, 0, 0);
        var amShiftEnd = new TimeSpan(13, 59, 59);
        var pmShiftStart = new TimeSpan(14, 0, 0);

        if (currentTimeOfDay >= amShiftStart && currentTimeOfDay <= amShiftEnd)
            currentShiftStartDateThreshold = nowUtc.Date + amShiftStart;
        else
            currentShiftStartDateThreshold = nowUtc.Date + pmShiftStart;

        var storeId = Ulid.Parse(storeDto.Id);

        storeDto.StoreInventories = await context.StoreInventories
                                                 .AsNoTracking()
                                                 .Where(si => si.StoreId == storeId)
                                                 .Select(si => new StoreInventoryDto
                                                     {
                                                         Id = si.Id.ToString(),
                                                         ProductId = si.Product.Id.ToString(),
                                                         ProductName = si.Product.Name,
                                                         ProductCategory = si.Product.Category,
                                                         Quantity = si.Quantity,
                                                         Price = si.Price,
                                                         LastVerified = si.LastVerified != null ? si.LastVerified.Value.ToString("o") : null
                                                     }
                                                 ).ToListAsync(cancellationToken);

        storeDto.Employees = await context.Employees
                                          .AsNoTracking()
                                          .Where(e => e.StoreId == storeId)
                                          .Select(e => new EmployeeDto
                                              {
                                                  Id = e.Id.ToString(),
                                                  FirstName = e.FirstName,
                                                  LastName = e.LastName,
                                                  Position = e.Position
                                              }
                                          ).ToListAsync(cancellationToken);

        storeDto.PreviousStoreInventories = await context.StoreInventories
                                                         .Where(si => si.StoreId == storeId)
                                                         .Where(si => si.LastVerified.HasValue && si.LastVerified.Value < currentShiftStartDateThreshold)
                                                         .Select(si => new StoreInventoryDto
                                                             {
                                                                 ProductId = si.Product.Id.ToString(),
                                                                 ProductName = si.Product.Name,
                                                                 ProductCategory = si.Product.Category,
                                                                 Quantity = si.Quantity,
                                                                 Price = si.Price,
                                                                 LastVerified = si.LastVerified != null ? si.LastVerified.Value.ToString("o") : null
                                                             }
                                                         ).OrderByDescending(si => si.LastVerified)
                                                         .ToListAsync(cancellationToken);

        return storeDto;

        Task<StoreDto?> StoreFromDbFactory() =>
            context.Stores.AsNoTracking()
                   .Where(s => s.StoreKey == request.StoreKey)
                   .Select(s => new StoreDto
                       {
                           Id = s.Id.ToString(),
                           StoreKey = s.StoreKey,
                           Name = s.Name,
                           Address = s.Address,
                           OpeningTime = s.OpeningTime,
                           ClosingTime = s.ClosingTime,
                           Employees = new List<EmployeeDto>(),
                           StoreInventories = new List<StoreInventoryDto>(),
                           PreviousStoreInventories = new List<StoreInventoryDto>()
                       }
                   )
                   .FirstOrDefaultAsync(cancellationToken);
    }
}
