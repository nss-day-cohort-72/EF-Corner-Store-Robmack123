namespace CornerStore.Models;

public class Cashier
{
    public int Id { get; set; }
    public string FirstName { get; set; }  // Not nullable
    public string LastName { get; set; }  // Not nullable
    public string FullName => $"{FirstName} {LastName}";

    public List<Order> Orders { get; set; } = new List<Order>();
}