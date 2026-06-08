namespace Orders.Api.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string CustomerName { get; private set; } = default!;
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    
    // EF Core needs parameterless constructor
    private Order(){}
    public Order(string customerName, decimal totalAmount)
    {
        Id = Guid.NewGuid();
        CustomerName = customerName;
        TotalAmount = totalAmount;
        CreatedAtUtc = DateTime.UtcNow;
    }
}