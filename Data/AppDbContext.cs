using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Erp.Models.ApplicationUsers;
using Erp.Models.Notify;
using Erp.Models.Products;
using Erp.Models.Purchase;
using Erp.Models.Sales;
using Erp.Models.Suppliers;

namespace Erp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Indexes
            builder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            builder.Entity<Supplier>()
                .HasIndex(s => s.Email)
                .IsUnique();

            // Relationships
            builder.Entity<PurchaseOrder>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseOrderItem>()
                .HasOne(i => i.PurchaseOrder)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.PurchaseOrderId);

            builder.Entity<PurchaseOrderItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(i => i.ProductId);

            builder.Entity<SalesOrder>()
                .HasOne(o => o.CreatedBy)
                .WithMany(u => u.SalesOrders)
                .HasForeignKey(o => o.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SalesOrderItem>()
                .HasOne(i => i.SalesOrder)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.SalesOrderId);

            builder.Entity<SalesOrderItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.SalesOrderItems)
                .HasForeignKey(i => i.ProductId);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId);
        }
    }
}