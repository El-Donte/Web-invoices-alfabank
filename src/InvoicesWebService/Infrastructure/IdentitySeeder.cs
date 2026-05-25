using System.Security.Claims;
using InvoicesWebService.Services.Authorization;
using Microsoft.AspNetCore.Identity;
using Shared.Entities;

namespace InvoicesWebService.Infrastructure;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndPermissionsAsync(this WebApplication application)
    {
        using var scope = application.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var syncService = scope.ServiceProvider.GetRequiredService<IPositionRoleSyncService>();

        var roles = new[]
        {
            "Admin",
            "Accountant",
            "Factoring",
            "Taxation", 
            "Acquiring" 
        };

        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is not null) continue;
            
            await roleManager.CreateAsync(role = new  IdentityRole<Guid>(roleName));

            await roleManager.AddClaimAsync(
                role,
                new Claim(CustomClaim.Permission, Permissions.InvoiceRead));
        }
        
        await CreateAdminUser(userManager, syncService);
    }
    
    private static async Task CreateAdminUser(UserManager<User> userManager, IPositionRoleSyncService syncService)
    {
        var admin = await userManager.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new User
            {
                UserName = "admin",
                Email = "admin@company.ru",
                FullName = "Администратор Системы",
                Position = Position.Admin,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, "Admin123!");
            await syncService.SyncPositionWithRolesAsync(admin);
        }
    }
}