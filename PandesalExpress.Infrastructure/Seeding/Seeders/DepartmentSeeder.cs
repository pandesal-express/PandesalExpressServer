using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Seeding.Abstractions;

namespace PandesalExpress.Infrastructure.Seeding.Seeders;

/// <summary>
/// Seeder for Department entities
/// </summary>
public class DepartmentSeeder(ILogger<DepartmentSeeder> logger) : BaseSeeder(logger)
{
    public override int Order => 1;

    public override string Name => "Department Seeder";

    protected override async Task SeedDataAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        Department[] departments =
        [
            new() { Id = Ulid.NewUlid(), Name = "Human Resources" },
            new() { Id = Ulid.NewUlid(), Name = "Information Technology" },
            new() { Id = Ulid.NewUlid(), Name = "Finance" },
            new() { Id = Ulid.NewUlid(), Name = "Store Operations" }, // Cashier, Baker, etc.
            new() { Id = Ulid.NewUlid(), Name = "Commissary" }
        ];
        
        // Check if any department exists
        if (await context.Departments.AnyAsync(cancellationToken))
        {
            Logger.LogInformation("Departments already exist, skipping seeding");
            return;
        }

        foreach (Department department in departments)
        {
            await AddIfNotExistsAsync(
                context.Departments,
                department,
                d => d.Name == department.Name,
                cancellationToken
            );
        }
    }
}