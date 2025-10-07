using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Services;
using Shared.Dtos;

namespace PandesalExpress.Management.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class DepartmentController(AppDbContext context, ICacheService cacheService) : ControllerBase
{
    private readonly Func<Task<List<DepartmentDto>>> _departmentsFactory = () => context.Departments
                                                                                        .AsNoTracking()
                                                                                        .Select(d => new DepartmentDto
                                                                                            {
                                                                                                Id = d.Id.ToString(),
                                                                                                Name = d.Name
                                                                                            }
                                                                                        )
                                                                                        .OrderBy(d => d.Name)
                                                                                        .ToListAsync();

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
    {
        const string cacheKey = "departments:all";

        List<DepartmentDto>? departments = await cacheService.GetOrSetAsync(
            cacheKey,
            _departmentsFactory,
            TimeSpan.FromHours(24)
        );

        return Ok(departments);
    }
}
