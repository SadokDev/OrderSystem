namespace Billing.Service.Data;
public class ProcessedMessage
{
    public Guid OrderId { get; set; }
    public DateTime ProcessedAt { get; set; }
}