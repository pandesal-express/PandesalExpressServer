using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Seeding.Abstractions;

namespace PandesalExpress.Infrastructure.Seeding.Seeders;

/// <summary>
/// Seeder for Role entities
/// </summary>
public class RoleSeeder(ILogger<RoleSeeder> logger, IServiceProvider serviceProvider) : BaseSeeder(logger)
{
    public override int Order => 2; // Run after departments

    public override string Name => "Role Seeder";

    protected override async Task SeedDataAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        // Check if any role exists
        if (await context.Roles.AnyAsync(cancellationToken))
        {
            Logger.LogInformation("Roles already exist, skipping seeding");
            return;
        }
        
        RoleManager<AppRole> roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();

        var roleNames = new[]
        {
            "Human Resources",
            "Information Technology", 
            "Finance",
            "Store Operations",
            "Commissary"
        };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                Logger.LogInformation("Creating role: {Role}", roleName);
                
                IdentityResult result = await roleManager.CreateAsync(new AppRole(roleName));
                if (!result.Succeeded)
                {
                    Logger.LogError("Failed to create role: {Role}. Errors: {Errors}", 
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                Logger.LogDebug("Role {Role} already exists", roleName);
            }
        }
    }
}