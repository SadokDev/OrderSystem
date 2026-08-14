using System.Text.Json;
using Orders.Api.Data;
using Orders.Api.Entities;
using OrderSystem.Contracts;

namespace Orders.Api.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (
            CreateOrderRequest request,
            ApplicationDbContext db,
            ILogger<Program> logger) =>
        {
            var order = new Order(request.CustomerName, request.TotalAmount);
            var correlationId = Guid.NewGuid();
            
            var eventMessage = new OrderCreated(
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                correlationId);

            var payload = JsonSerializer.Serialize(eventMessage);

            db.Orders.Add(order);

            logger.LogInformation(
                "Creating Order with CorrelationId {CorrelationId}",
                correlationId);

            db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = typeof(OrderCreated).FullName!,
                Payload = payload
            });

            await db.SaveChangesAsync();

            return Results.Created($"/orders/{order.Id}", order);
        });
    }
}