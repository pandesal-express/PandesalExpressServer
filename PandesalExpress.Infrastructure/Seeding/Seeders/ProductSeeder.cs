using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Seeding.Abstractions;

namespace PandesalExpress.Infrastructure.Seeding.Seeders;

/// <summary>
/// Seeder for Product entities
/// </summary>
public class ProductSeeder(ILogger<ProductSeeder> logger) : BaseSeeder(logger)
{
    private readonly string[] _drinkNames =
    [
        "Coke",
        "Sprite", 
        "Pepsi",
        "Water Bottle",
        "Iced Tea"
    ];

    private readonly string[] _pandesalProducts =
    [
        // Flavored Pandesal (Plain Flavor Infused in Dough)
        "Ube Pandesal",
        "Cheese Pandesal",
        "Pandan Pandesal",
        "Choco Pandesal",
        "Strawberry Pandesal",
        "Matcha Pandesal",
        "Cinnamon Pandesal",
        "Coffee Pandesal",
        "Banana Pandesal",

        // Premium Filled Pandesal
        "Ube-Cheese Filled Pandesal",
        "Cheese-Cheese Filled Pandesal (Double Cheese)",
        "Pandan-Cheese Filled Pandesal",
        "Coffee-Cheese Filled Pandesal",
        "Choco-Choco Filled Pandesal",
        "Strawberry-Choco Filled Pandesal",
        "Coffee-Choco Filled Pandesal",
        "Matcha-Choco Filled Pandesal",
        "Cinna-Choco Filled Pandesal",
        "Ube-Ube Filled Pandesal (Ultimate Ube)",
        "Cheese-Ube Filled Pandesal",
        "Pandan-Ube Filled Pandesal",
        "Pandan-Pandan Filled Pandesal",
        "Banana-Choco Filled Pandesal",
        "Chicken Adobo Pandesal",
        "Pan de Coco (Coconut Filled)",
        "Salted Egg Pandesal",

        // Ultimate/Specialty Pandesal
        "Double Cheese Ube Filled Pandesal",
        "Ube Ube-Cheese Combo Pandesal",
        "Ultimate Ube Ube Pandesal",
        "Double Ube Cheese Combo Pandesal",

        // Other Breads & Rolls
        "Spanish Bread",
        "Pan de Leche",
        "Hawaiian Sweet Rolls",
        "Milk Raisin Sweet Roll",
        "Ensaymada (Plain)",
        "Ensaymada (Monggo)",
        "Ensaymada (Cheese)",
        "Monay Bread",
        "King Roll",
        "Ube Bread (Loaf/Roll)",
        "Cheese Roll Bread",
        "Buko Bread",
        "Chocolate Chip Brioche Rolls",
        "Cardamom Raisin Sweet Rolls",
        "Julekake Fruit Sweet Rolls",
        "Hot Cross Rolls",
        "Mexican Crackle Top Bread",

        // Pastries & Other Items (often found in Filipino Bakeries)
        "Bizkotso (Biscocho)",
        "Toasted Siopao",
        "Leche Flan",
        "Cassava Cake",
        "Goldi Hopia Pork",
        "Polvoron",
        "Bibingka",
        "Buchi",
        "Italian Bomboloni",
        "Biko"
    ];

    public override int Order => 4; // Run after departments, roles, and stores

    public override string Name => "Product Seeder";

    protected override async Task SeedDataAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        // check if any product exists
        if (await context.Products.AnyAsync(cancellationToken))
        {
            Logger.LogInformation("Products already exist, skipping seeding");
            return;
        }
        
        var random = new Random();
        var products = new List<Product>();

        // Add pandesal products
        for (int i = 0; i < _pandesalProducts.Length; i++)
        {
            var productName = _pandesalProducts[i];
            var shift = i <= _pandesalProducts.Length / 2 ? "AM" : "PM";

            // Randomly assign some products to "Both" shifts
            if (random.Next(1, 10) <= 3)
                shift = "Both";

            var product = new Product
            {
                Id = Ulid.NewUlid(),
                Name = productName,
                Category = "Foods",
                Price = Math.Round(random.Next(500, 2500) / 100.0m, 2),
                Shift = shift,
                Quantity = random.Next(30, 100),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            products.Add(product);
        }

        // Add drink products
        products.AddRange(
            _drinkNames.Select(drinkName => new Product
                {
                    Id = Ulid.NewUlid(),
                    Name = drinkName,
                    Category = "Drinks",
                    Price = 20.00m,
                    Shift = "Both",
                    Quantity = random.Next(30, 50),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            )
        );

        // Add all products to the database
        foreach (Product product in products)
        {
            await AddIfNotExistsAsync(
                context.Products,
                product,
                p => p.Name == product.Name,
                cancellationToken
            );
        }
    }
}