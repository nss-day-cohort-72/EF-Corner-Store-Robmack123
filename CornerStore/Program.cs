using CornerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using CornerStore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//endpoints go here

//Cashier endpoints

//Get all Cashiers
app.MapGet("/api/cashiers", async ( CornerStoreDbContext dbContext) =>
{
    var cashiers = await dbContext.Cashiers.ToListAsync();
    return Results.Ok(cashiers);
});

// Get a cashier and their orders
app.MapGet("/api/cashiers/{int}", async (int id, CornerStoreDbContext dbContext) =>
{
    var cashier = await dbContext.Cashiers
        .Include(c => c.Orders)
        .ThenInclude(c => c.OrderProducts)
        .ThenInclude(op => op.Product)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (cashier == null)
    {
        return Results.NotFound($"Cashier with ID {id} not found");
    }

    return Results.Ok(cashier);
});

//create a new cashier
app.MapPost("/api/cashiers", async (CreateCashierDTO newCashierDTO, CornerStoreDbContext dbContext) =>
{
    if (string.IsNullOrWhiteSpace(newCashierDTO.FirstName) || string.IsNullOrWhiteSpace(newCashierDTO.LastName))
    {
        return Results.NotFound("Cashier FirstName and LastName needed");
    }

    var newCashier = new Cashier
    {
        FirstName = newCashierDTO.FirstName,
        LastName = newCashierDTO.LastName
    };

    dbContext.Cashiers.Add(newCashier);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/cashiers/{newCashier.Id}", newCashier);
});

// Product endpoints

// Get a list of all products or use a string to find a specific product or category
app.MapGet("/api/products", async (string? search, CornerStoreDbContext dbContext) =>
{
    var query = dbContext.Products
        .Include(p => p.Category)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        string searchLower = search.ToLower();
        query = query.Where(p => p.ProductName.ToLower().Contains(searchLower) || p.Category.CategoryName.ToLower().Contains(searchLower));
    }

    var products = await query.ToListAsync();

    return Results.Ok(products);
});

// Post a new product
app.MapPost("/api/products", async (CreateProductDTO newProductDTO, CornerStoreDbContext dbContext) =>
{
    if (string.IsNullOrWhiteSpace(newProductDTO.ProductName) || string.IsNullOrWhiteSpace(newProductDTO.Brand) || newProductDTO.Price <= 0)
    {
        return Results.BadRequest("Product Name, valid price, and brand are required");
    }

    var newProduct = new Product
    {
        ProductName = newProductDTO.ProductName,
        Price = newProductDTO.Price,
        Brand = newProductDTO.Brand,
        CategoryId = newProductDTO.CategoryId
    };
    
    dbContext.Products.Add(newProduct);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/products/{newProduct.Id}", newProduct);
});

//Update the products price
app.MapPut("/api/services/{id}", async (int id, UpdateProductDTO updatedProduct, CornerStoreDbContext dbContext) =>
{
    var product = await dbContext.Products.FindAsync(id);
    if (product == null)
    {
        return Results.NotFound($"Service with ID {id} not found");
    }

    product.Price = updatedProduct.Price;

    await dbContext.SaveChangesAsync();

    return Results.Ok(product);
});
// Orders endpoints

//get an orders details
app.MapGet("/api/orders/{id}", async (int id, CornerStoreDbContext dbContext) =>
{
    var order = await dbContext.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .ThenInclude(p => p.Category)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound($"Order with ID {id} not found.");
    }

    var orderDetailsDTO = new OrderDetailsDTO
     {
        Id = order.Id,
        PaidOnDate = order.PaidOnDate,
        Total = order.Total,
        Cashier = new CashierDTO
        {
            Id = order.Cashier.Id,
            FirstName = order.Cashier.FirstName,
            LastName = order.Cashier.LastName
        },
        OrderProducts = order.OrderProducts.Select(op => new OrderProductDTO
        {
            ProductId = op.ProductId,
            Quantity = op.Quantity,
            Product = new ProductDTO
            {
                Id = op.Product.Id,
                ProductName = op.Product.ProductName,
                Price = op.Product.Price,
                Brand = op.Product.Brand,
                Category = new CategoryDTO
                {
                    Id = op.Product.Category.Id,
                    CategoryName = op.Product.Category.CategoryName
                }
            }
        }).ToList()
    };

    return Results.Ok(orderDetailsDTO);
});

//get all orders
app.MapGet("/api/orders", async (DateTime? orderDate, CornerStoreDbContext dbContext) =>
{
    // Start the query for orders
    var query = dbContext.Orders
        .Include(o => o.Cashier) 
        .Include(o => o.OrderProducts) 
            .ThenInclude(op => op.Product) 
            .ThenInclude(p => p.Category)                                                       
        .AsQueryable();

    if (orderDate.HasValue)
    {
        query = query.Where(o => o.PaidOnDate.HasValue && o.PaidOnDate.Value.Date == orderDate.Value.Date);
    }

    var orders = await query.ToListAsync();

    return Results.Ok(orders);
});

app.MapDelete("/api/orders/{id}", async (int id, CornerStoreDbContext dbContext) =>
{
    var order = await dbContext.Orders
        .Include(o => o.OrderProducts)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound($"Order with ID {id} not found ");
    }

    dbContext.Orders.Remove(order);

    await dbContext.SaveChangesAsync();
    
    return Results.Ok($"Order with ID {id} was deleted successfully.");
});

app.MapPost("/api/orders", async (CreateOrderDTO newOrderDTO, CornerStoreDbContext dbContext) =>
{
    // Validate Cashier exists
    var cashier = await dbContext.Cashiers.FindAsync(newOrderDTO.CashierId);
    if (cashier == null)
    {
        return Results.NotFound($"Cashier with ID {newOrderDTO.CashierId} not found.");
    }

    // Validate all ProductIds exist
    var productIds = newOrderDTO.Products.Select(p => p.ProductId).ToList();
    var validProducts = await dbContext.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
    if (validProducts.Count != productIds.Count)
    {
        return Results.BadRequest("One or more ProductIds are invalid.");
    }

    // Create the Order
    var order = new Order
    {
        CashierId = newOrderDTO.CashierId,
        PaidOnDate = newOrderDTO.PaidOnDate,
        OrderProducts = newOrderDTO.Products.Select(p => new OrderProduct
        {
            ProductId = p.ProductId,
            Quantity = p.Quantity
        }).ToList()
    };

    dbContext.Orders.Add(order);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/orders/{order.Id}", order);
});

app.Run();



//don't move or change this!
public partial class Program { }