using Microsoft.EntityFrameworkCore;
using Orders.Api.Contracts;
using Orders.Api.Data;
using Orders.Api.Entities;

namespace Orders.Api.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (
            CreateOrderRequest request,
            ApplicationDbContext db) =>
        {
            var order = new Order(request.CustomerName, request.TotalAmount);

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            return Results.Created($"/orders/{order.Id}", order);
        });
    }
}