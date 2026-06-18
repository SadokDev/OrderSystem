using MassTransit;
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
            IPublishEndpoint publishEndpoint) =>
        {
            var order = new Order(request.CustomerName, request.TotalAmount);

            db.Orders.Add(order);
            await db.SaveChangesAsync();
            
            await publishEndpoint.Publish(new OrderCreated(
                order.Id,
                order.CustomerName,
                order.TotalAmount));

            return Results.Created($"/orders/{order.Id}", order);
        });
    }
}