namespace CornerStore.Models
{
    public class OrderProductDTO
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public ProductDTO Product { get; set; }
}
}