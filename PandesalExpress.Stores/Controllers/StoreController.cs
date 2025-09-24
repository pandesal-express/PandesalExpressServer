using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Services;
using PandesalExpress.Stores.Features.GetStoreByKey;
using Shared.Dtos;

namespace PandesalExpress.Stores.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class StoreController(
    AppDbContext context,
    ICacheService cacheService
) : ControllerBase
{
    // GET api/Store
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StoreDto>>> GetAllStores()
    {
        const string cacheKey = "stores:all-stores";

        List<StoreDto>? stores = await cacheService.GetOrSetAsync(
            cacheKey,
            (Func<Task<List<StoreDto>>>)StoresFromDbFactory,
            TimeSpan.FromMinutes(10)
        );

        return Ok(stores);

        Task<List<StoreDto>> StoresFromDbFactory() =>
            context.Stores.AsNoTracking()
                   .Select(s => new StoreDto
                       {
                           Id = s.Id.ToString(),
                           StoreKey = s.StoreKey,
                           Name = s.Name,
                           Address = s.Address,
                           StocksDateVerified = s.StocksDateVerified.HasValue &&
                                                s.StocksDateVerified.Value.Date == DateTime.UtcNow.Date
                               ? "Verified Today"
                               : "Verification Needed"
                       }
                   )
                   .OrderBy(s => s.Name)
                   .ToListAsync();
    }

    [HttpGet("{storeKey}")]
    [ProducesResponseType(typeof(StoreDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetStoreByKey(string storeKey, [FromServices] IMediator mediator)
    {
        var query = new GetStoreByKeyQuery(storeKey);
        StoreDto? result = await mediator.Send(query, HttpContext.RequestAborted);

        return result is not null ? Ok(result) : NotFound();
    }
}
