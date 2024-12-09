using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
public class CornerStoreDbContext : DbContext
{
    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {

    }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new { op.OrderId, op.ProductId });

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, CategoryName = "Beverages" },
            new Category { Id = 2, CategoryName = "Snacks" }
        );

        modelBuilder.Entity<Cashier>().HasData(
            new Cashier { Id = 1, FirstName = "James", LastName = "Smith" },
            new Cashier { Id = 2, FirstName = "Sarah", LastName = "Johnson" }
        );
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, ProductName = "Coca-Cola", Price = 1.5m, Brand = "Coca-Cola", CategoryId = 1 },
            new Product { Id = 2, ProductName = "Pepsi", Price = 1.4m, Brand = "Pepsi", CategoryId = 1 },
            new Product { Id = 3, ProductName = "Lays Chips", Price = 2.0m, Brand = "Lays", CategoryId = 2 }
        );

        // Seed Orders
        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 1, CashierId = 1, PaidOnDate = DateTime.Now },
            new Order { Id = 2, CashierId = 2, PaidOnDate = DateTime.Now.AddDays(-1) }
        );

        // Seed OrderProducts
        modelBuilder.Entity<OrderProduct>().HasData(
            new OrderProduct { ProductId = 1, OrderId = 1, Quantity = 2 },
            new OrderProduct { ProductId = 3, OrderId = 1, Quantity = 1 },
            new OrderProduct { ProductId = 2, OrderId = 2, Quantity = 3 }
        );
        }
}