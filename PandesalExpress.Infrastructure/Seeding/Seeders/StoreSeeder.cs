using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Seeding.Abstractions;

namespace PandesalExpress.Infrastructure.Seeding.Seeders;

/// <summary>
/// Seeder for Store entities
/// </summary>
public class StoreSeeder(ILogger<StoreSeeder> logger) : BaseSeeder(logger)
{
    public override int Order => 3; // Run after departments and roles

    public override string Name => "Store Seeder";

    protected override async Task SeedDataAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        // Check if any store exists
        if (await context.Stores.AnyAsync(cancellationToken))
        {
            Logger.LogInformation("Stores already exist, skipping seeding");
            return;
        }
        
        Store[] stores =
        [
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX001",
                Name = "Pandesal Bakery Express Bacolod - Downtown",
                Address = "123 Rizal Street, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX002",
                Name = "Pandesal Bakery Express Bacolod - Mandalagan",
                Address = "456 Lopez Jaena Street, Mandalagan, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX003",
                Name = "Pandesal Bakery Express Bacolod - Granada",
                Address = "789 Quirino Avenue, Granada, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX004",
                Name = "Pandesal Bakery Express Bacolod - Sum-ag",
                Address = "1010 Locsin Street, Sum-ag, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX005",
                Name = "Pandesal Bakery Express Bacolod - Bata",
                Address = "1111 Lacson Street, Bata, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX006",
                Name = "Pandesal Bakery Express Bacolod - Alijis",
                Address = "1212 Araneta Avenue, Alijis, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX007",
                Name = "Pandesal Bakery Express Bacolod - Bredco",
                Address = "1313 Dizon Street, Bredco, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX008",
                Name = "Pandesal Bakery Express Bacolod - Airport Area",
                Address = "1414 Ballesteros Avenue, Airport Area, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX009",
                Name = "Pandesal Bakery Express Bacolod - Vista Alegre",
                Address = "1515 Mabini Street, Vista Alegre, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX010",
                Name = "Pandesal Bakery Express Bacolod - West District",
                Address = "1616 Legaspi Street, West District, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX011",
                Name = "Pandesal Bakery Express Bacolod - East District",
                Address = "1717 Rizal Avenue, East District, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX012",
                Name = "Pandesal Bakery Express Bacolod - South District",
                Address = "1818 Bonifacio Street, South District, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX013",
                Name = "Pandesal Bakery Express Bacolod - North District",
                Address = "1919 Quezon Street, North District, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX014",
                Name = "Pandesal Bakery Express Bacolod - Mandalapit",
                Address = "2020 Freedom Lane, Mandalapit, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            },
            new Store
            {
                Id = Ulid.NewUlid(),
                StoreKey = "PBEX015",
                Name = "Pandesal Bakery Express Bacolod - Manokan",
                Address = "2121 San Juan Street, Manokan, Bacolod City, Philippines",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            }
        ];

        foreach (Store store in stores)
        {
            await AddIfNotExistsAsync(
                context.Stores,
                store,
                s => s.StoreKey == store.StoreKey,
                cancellationToken
            );
        }
    }
}