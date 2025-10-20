using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Erp.Data;
using Erp.Data.Enum;
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
        foreach (var roleName in Enum.GetNames(typeof(AppRoles)))
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
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
               new Supplier
               {
                   Id = Guid.NewGuid(),
                   Name = "TechWorld Distributors",
                   ContactPerson = "John Doe",
                   Email = "sales@techworld.com",
                   Phone = "555-1111",
                   Address = "New York, USA"
               },
                new Supplier
                {
                    Id = Guid.NewGuid(),
                    Name = "Global Components",
                    ContactPerson = "Anna Schmidt",
                    Email = "info@globalcomponents.com",
                    Phone = "555-2222",
                    Address = "Berlin, Germany"
                }
            );
            await dbContext.SaveChangesAsync();
            Console.WriteLine("✅ Seeded sample suppliers.");
        }
        #endregion

        #region Seed Product sample Data
        if (!await dbContext.Products.AnyAsync())
        {
            dbContext.Products.AddRange(
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Wireless Mouse",
                    SKU = "PRD-001",
                    Description = "Ergonomic wireless mouse with adjustable DPI",
                    Price = 25,
                    StockQuantity = 100,
                    ReorderLevel = 10,
                    IsActive = true
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Mechanical Keyboard",
                    SKU = "PRD-002",
                    Description = "RGB backlit mechanical keyboard with blue switches",
                    Price = 70,
                    StockQuantity = 50,
                    ReorderLevel = 5,
                    IsActive = true
                }
            );
            await dbContext.SaveChangesAsync();
            Console.WriteLine("✅ Seeded sample products.");
        }
        #endregion
        Console.WriteLine("🎉 Database seeding completed successfully!");
    }
}
