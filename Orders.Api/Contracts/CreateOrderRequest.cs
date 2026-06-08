namespace Orders.Api.Contracts;

public record CreateOrderRequest(
    string CustomerName,
    decimal TotalAmount
);