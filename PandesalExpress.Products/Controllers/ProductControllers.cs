using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Services;
using Shared.Dtos;

namespace PandesalExpress.Products.Controllers;

[Authorize] 
[Route("api/[controller]")]
[ApiController]
public class ProductController(
    AppDbContext context, 
    ICacheService cacheService,
    IShiftService shiftService
) : ControllerBase
{
    private Task<List<ProductDto>> _productsFactory(string shift = "Both") =>
        context.Products.AsNoTracking()
               .OrderBy(p => p.Name)
               .Where(p => shift == "Both" || p.Shift == shift)
               .Select(p => new ProductDto
                   {
                       Id = p.Id.ToString(),
                       Category = p.Category,
                       Name = p.Name,
                       Price = p.Price,
                       Quantity = p.Quantity,
                       Shift = p.Shift,
                       Description = p.Description
                   }
               ).ToListAsync();
    
    // GET: api/Products
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
    {
        const string cacheKey = "products:all";
        
        List<ProductDto>? products = await cacheService.GetOrSetAsync(
            cacheKey, 
            () => _productsFactory(), 
            TimeSpan.FromHours(1)
        );

        return Ok(products);
    }

    [HttpGet("for-shift")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> GetProductsForCurrentShift()
    {
        ShiftType currentShift = shiftService.GetCurrentShift();
        string currentShiftString = currentShift.ToString().ToUpper();
        string cacheKey = $"products:shift:{currentShiftString}";
        
        List<ProductDto>? products = await cacheService.GetOrSetAsync(
            cacheKey, 
            () => _productsFactory(currentShiftString), 
            TimeSpan.FromMinutes(30)
        );

        return Ok(products);
    }
}
