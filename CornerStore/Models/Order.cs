
namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }

    // Foreign key
    public int CashierId { get; set; }

    // Navigation property: An order is handled by one cashier
    public Cashier Cashier { get; set; } = null!;

    // Computed property for total
    public decimal Total => OrderProducts.Sum(op => op.Product.Price * op.Quantity);

    // Nullable DateTime for when the transaction was completed
    public DateTime? PaidOnDate { get; set; }

    // Navigation property: An order can have many associated order products
    public List<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}