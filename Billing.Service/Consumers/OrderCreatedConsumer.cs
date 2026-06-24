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
        var messageId = context.Message.OrderId;

        Console.WriteLine($"💳 Processing Order {messageId}");

        // 1. Idempotence check (DB-level safety)
        var alreadyProcessed = await _db.ProcessedMessages
            .AnyAsync(x => x.OrderId == messageId);

        if (alreadyProcessed)
        {
            Console.WriteLine($"⚠️ Duplicate skipped {messageId}");
            return;
        }

        // 2. Persist idempotence marker
        _db.ProcessedMessages.Add(new ProcessedMessage
        {
            OrderId = messageId,
            ProcessedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        Console.WriteLine($"✅ Order {messageId} processed successfully");
    }
}