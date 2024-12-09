namespace CornerStore.Models
{
    public class OrderDetailsDTO
{
    public int Id { get; set; }
    public DateTime? PaidOnDate { get; set; }
    public decimal Total { get; set; }
    public CashierDTO Cashier { get; set; }
    public List<OrderProductDTO> OrderProducts { get; set; }
}
}