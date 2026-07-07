using Billing.Service.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Contracts;

namespace Billing.Service.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly BillingDbContext _db;

    public OrderCreatedConsumer(BillingDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var message = context.Message;
        
        Console.WriteLine(
            $"[Header CorrelationId: {context.CorrelationId}]");

        Console.WriteLine(
            $"[CorrelationId: {message.CorrelationId}] Processing Order {message.OrderId}");

        var messageId = message.OrderId;

        // 1. Idempotence check (DB-level safety)
        var alreadyProcessed = await _db.ProcessedMessages
            .AnyAsync(x => x.OrderId == messageId);

        if (alreadyProcessed)
        {
            Console.WriteLine(
                $"[CorrelationId: {message.CorrelationId}] Duplicate skipped {messageId}");
            return;
        }

        // 2. Persist idempotence marker
        _db.ProcessedMessages.Add(new ProcessedMessage
        {
            OrderId = messageId,
            ProcessedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        Console.WriteLine(
            $"[CorrelationId: {message.CorrelationId}] Processed Order {messageId} successfully");
    }
}