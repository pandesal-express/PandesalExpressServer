using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Context;

namespace PandesalExpress.Infrastructure.Seeding.Abstractions;

/// <summary>
/// Base class for database seeders providing common functionality
/// </summary>
public abstract class BaseSeeder(ILogger logger) : ISeeder
{
    protected readonly ILogger Logger = logger;

    /// <summary>
    /// Seeds data into the database
    /// </summary>
    public async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Starting seeding for {SeederName}", Name);
            
            await SeedDataAsync(context, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            
            Logger.LogInformation("Completed seeding for {SeederName}", Name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while seeding {SeederName}", Name);
            throw;
        }
    }

    /// <summary>
    /// The actual seeding logic
    /// </summary>
    protected abstract Task SeedDataAsync(AppDbContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the order in which this seeder should run (lower numbers run first)
    /// </summary>
    public abstract int Order { get; }

    /// <summary>
    /// Gets the name of this seeder for logging purposes
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Helper method to check if an entity exists by a predicate
    /// </summary>
    private async Task<bool> ExistsAsync<T>(
        DbSet<T> dbSet, 
        System.Linq.Expressions.Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default
    ) where T : class => await dbSet.AnyAsync(predicate, cancellationToken);

    /// <summary>
    /// Helper method to add entity only if it doesn't exist
    /// </summary>
    protected async Task AddIfNotExistsAsync<T>(
        DbSet<T> dbSet, T entity, 
        System.Linq.Expressions.Expression<Func<T, bool>> existsPredicate, 
        CancellationToken cancellationToken = default
    ) where T : class
    {
        if (!await ExistsAsync(dbSet, existsPredicate, cancellationToken))
        {
            await dbSet.AddAsync(entity, cancellationToken);
            Logger.LogDebug("Added {EntityType}: {Entity}", typeof(T).Name, entity);
        }
        else
        {
            Logger.LogDebug("Skipped existing {EntityType}", typeof(T).Name);
        }
    }
}