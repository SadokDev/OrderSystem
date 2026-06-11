namespace Orders.Api.Contracts;

public record OrderCreated(
    Guid OrderId,
    string CustomerName,
    decimal Amount);