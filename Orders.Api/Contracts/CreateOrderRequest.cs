namespace Orders.Api;

public record CreateOrderRequest(
    string CustomerName,
    decimal TotalAmount);