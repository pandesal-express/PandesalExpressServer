# Database Seeding

This directory contains the database seeding infrastructure for PandesalExpress. The seeding system allows you to populate the database with initial or sample data in a structured and organized way.

## Structure

```
Seeding/
├── Abstractions/           # Base interfaces and classes
│   ├── ISeeder.cs         # Interface for all seeders
│   └── BaseSeeder.cs      # Base class with common functionality
├── Data/                  # Static data files (JSON, CSV, etc.)
├── Extensions/            # Extension methods for DI registration
│   └── DatabaseSeedingExtensions.cs
├── Seeders/               # Individual seeder implementations
│   └── DepartmentSeeder.cs
└── Services/              # Core seeding services
    └── DatabaseSeedingService.cs
```

## Usage

### 1. Register Seeding Services

In your `Program.cs` or startup configuration:

```csharp
// Add seeding services
builder.Services.AddDatabaseSeeding();

// Register individual seeders in order
builder.Services.AddSeeder<DepartmentSeeder>();
builder.Services.AddSeeder<RoleSeeder>();
builder.Services.AddSeeder<StoreSeeder>();
builder.Services.AddSeeder<ProductSeeder>();
// ... add more seeders

// Run seeding during application startup (manual approach)
if (app.Environment.IsDevelopment())
{
    await app.Services.SeedDatabaseAsync();
}
```

### 2. Create a New Seeder

```csharp
public class ProductSeeder : BaseSeeder
{
    public ProductSeeder(ILogger<ProductSeeder> logger) : base(logger)
    {
    }

    public override int Order => 3; // Run after departments and stores
    public override string Name => "Product Seeder";

    protected override async Task SeedDataAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var products = new[]
        {
            new Product 
            { 
                Id = Ulid.NewUlid(), 
                Name = "Pandesal", 
                Price = 2.50m,
                Category = "Bread",
                Shift = "AM"
            }
        };

        foreach (var product in products)
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
```

### 3. Manual Seeding

You can also run seeding manually:

```csharp
// Run all seeders
await serviceProvider.SeedDatabaseAsync();

// Run specific seeder
await serviceProvider.SeedDatabaseAsync("Department Seeder");
```

## Best Practices

1. **Order Matters**: Set the `Order` property appropriately. Entities with dependencies should have higher order numbers.

2. **Idempotent Operations**: Always check if data exists before adding it using `AddIfNotExistsAsync()` or similar patterns.

3. **Use Static Data Files**: For large datasets, consider storing data in JSON or CSV files in the `Data/` folder and loading them in your seeders.

4. **Environment-Specific**: Control when seeding runs by checking the environment in your Host application (e.g., `app.Environment.IsDevelopment()`).

5. **Logging**: The base seeder provides logging. Use it to track seeding progress and debug issues.

## Data Files

Store static data in the `Data/` folder:

```
Data/
├── departments.json
├── products.csv
└── sample-employees.json
```

Load them in your seeders:

```csharp
var json = await File.ReadAllTextAsync("Seeding/Data/departments.json");
var departments = JsonSerializer.Deserialize<Department[]>(json);
```

## Common Seeding Order

1. **Departments** (Order: 1) - Base organizational units
2. **Roles** (Order: 2) - ASP.NET Core Identity roles based on departments  
3. **Stores** (Order: 3) - Physical store locations
4. **Products** (Order: 4) - Pandesal products and drinks
5. **Employees** (Order: 5) - depends on Departments and Stores (when implemented)
6. **PDND Requests** (Order: 6) - depends on Employees, Stores, Products (when implemented)

## Error Handling

The seeding system includes comprehensive error handling and logging. If a seeder fails, the entire seeding process will stop and log the error details.