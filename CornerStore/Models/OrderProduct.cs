namespace CornerStore.Models;


public class OrderProduct
{
    // Foreign keys
    public int ProductId { get; set; }
    public int OrderId { get; set; }


    public Product Product { get; set; } 
    public Order Order { get; set; } 

    public int Quantity { get; set; }
}