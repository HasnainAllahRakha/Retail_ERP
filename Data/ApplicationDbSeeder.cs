using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Erp.Data;
using Erp.Models.ApplicationUsers;
using Erp.Models.Suppliers;
using Erp.Models.Products;

public static class ApplicationDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        #region Seed Roles
        string[] roles = { "Admin", "Staff" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"✅ Created Role: {role}");
            }
        }
        #endregion

        #region Seed Default Admin
        if (!await userManager.Users.AnyAsync())
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@erp.com",
                Email = "admin@erp.com",
                FullName = "System Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("✅ Default admin created: admin@erp.com / Admin@123");
            }
            else
            {
                Console.WriteLine("❌ Failed to create default admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        #endregion

        #region Seed Supplier sample Data
        if (!await dbContext.Suppliers.AnyAsync())
        {
            dbContext.Suppliers.AddRange(
                new Supplier { Name = "TechWorld Distributors", Email = "sales@techworld.com", Phone = "555-1111", Address = "New York, USA" },
                new Supplier { Name = "Global Components", Email = "info@globalcomponents.com", Phone = "555-2222", Address = "Berlin, Germany" }
            );
            await dbContext.SaveChangesAsync();
            Console.WriteLine("✅ Seeded sample suppliers.");
        }
        #endregion

        #region Seed Product sample Data
        if (!await dbContext.Products.AnyAsync())
        {
            dbContext.Products.AddRange(
                new Product { Name = "Wireless Mouse", SKU = "PRD-001", Price = 25, StockQuantity = 100, ReorderLevel = 10 },
                new Product { Name = "Mechanical Keyboard", SKU = "PRD-002", Price = 70, StockQuantity = 50, ReorderLevel = 5 }
            );
            await dbContext.SaveChangesAsync();
            Console.WriteLine("✅ Seeded sample products.");
        }
        #endregion
        Console.WriteLine("🎉 Database seeding completed successfully!");
    }
}
