using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Seeding.Abstractions;

namespace PandesalExpress.Infrastructure.Seeding.Services;

/// <summary>
/// Service responsible for coordinating database seeding operations
/// </summary>
public class DatabaseSeedingService(IServiceProvider serviceProvider, ILogger<DatabaseSeedingService> logger)
{
    /// <summary>
    /// Runs all registered seeders in order
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seeders = scope.ServiceProvider.GetServices<ISeeder>()
            .OrderBy(s => s.Order)
            .ToList();

        if (seeders.Count == 0)
        {
            logger.LogInformation("No seeders registered");
            return;
        }

        logger.LogInformation("Starting database seeding with {SeederCount} seeders", seeders.Count);

        foreach (ISeeder seeder in seeders)
        {
            try
            {
                await seeder.SeedAsync(context, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to run seeder {SeederName}", seeder.Name);
                throw;
            }
        }

        logger.LogInformation("Database seeding completed successfully");
    }

    /// <summary>
    /// Runs a specific seeder by name
    /// </summary>
    public async Task SeedAsync(string seederName, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ISeeder? seeder = scope.ServiceProvider.GetServices<ISeeder>()
                               .FirstOrDefault(s => s.Name.Equals(seederName, StringComparison.OrdinalIgnoreCase));

        if (seeder == null)
        {
            logger.LogWarning("Seeder {SeederName} not found", seederName);
            return;
        }

        logger.LogInformation("Running specific seeder: {SeederName}", seederName);
        await seeder.SeedAsync(context, cancellationToken);
        logger.LogInformation("Completed seeder: {SeederName}", seederName);
    }
}