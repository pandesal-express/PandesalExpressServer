using Microsoft.Extensions.DependencyInjection;
using PandesalExpress.Infrastructure.Seeding.Abstractions;
using PandesalExpress.Infrastructure.Seeding.Services;

namespace PandesalExpress.Infrastructure.Seeding.Extensions;

/// <summary>
/// Extension methods for configuring database seeding
/// </summary>
public static class DatabaseSeedingExtensions
{
    /// <summary>
    /// Adds database seeding services to the service collection
    /// </summary>
    public static IServiceCollection AddDatabaseSeeding(this IServiceCollection services)
    {
        services.AddScoped<DatabaseSeedingService>();
        return services;
    }

    /// <summary>
    /// Registers a seeder with the service collection
    /// </summary>
    public static IServiceCollection AddSeeder<TSeeder>(this IServiceCollection services)
        where TSeeder : class, ISeeder
    {
        services.AddScoped<ISeeder, TSeeder>();
        return services;
    }

    /// <summary>
    /// Runs database seeding manually
    /// </summary>
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        DatabaseSeedingService seedingService = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
        await seedingService.SeedAsync(cancellationToken);
    }

    /// <summary>
    /// Runs a specific seeder manually
    /// </summary>
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider, string seederName, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        DatabaseSeedingService seedingService = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
        await seedingService.SeedAsync(seederName, cancellationToken);
    }
}