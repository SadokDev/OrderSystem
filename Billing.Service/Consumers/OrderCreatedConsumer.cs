using MassTransit;
using Orders.Api.Contracts;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    public Task Consume(ConsumeContext<OrderCreated> context)
    {
        Console.WriteLine($"💳 Processing payment for Order {context.Message.OrderId}");
        Console.WriteLine($"Customer: {context.Message.CustomerName}");
        Console.WriteLine($"Amount: {context.Message.Amount}");

        return Task.CompletedTask;
    }
}