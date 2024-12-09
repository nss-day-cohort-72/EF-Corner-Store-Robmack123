namespace CornerStore.Models
{
    public class CreateOrderDTO
    {
        public int CashierId { get; set; }
        public DateTime? PaidOnDate { get; set; }
        public List<OrderProductDTO> Products { get; set; }
    }
}