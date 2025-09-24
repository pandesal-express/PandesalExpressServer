using PandesalExpress.Infrastructure.Context;

namespace PandesalExpress.Infrastructure.Seeding.Abstractions;

/// <summary>
/// Interface for database seeders
/// </summary>
public interface ISeeder
{
    /// <summary>
    /// Seeds data into the database
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the seeding operation</returns>
    Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the order in which this seeder should run (lower numbers run first)
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Gets the name of this seeder for logging purposes
    /// </summary>
    string Name { get; }
}