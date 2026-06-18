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

        try
        {
            // 1. Persist idempotence marker FIRST
            _db.ProcessedMessages.Add(new ProcessedMessage
            {
                OrderId = messageId,
                ProcessedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            Console.WriteLine($"✅ Order {messageId} saved in DB");

            // 2. CRASH AFTER COMMIT (simulate real-world failure)
            throw new Exception("💥 Crash AFTER SaveChanges (simulated failure)");
        }
        catch (DbUpdateException)
        {
            Console.WriteLine("⚠️ Duplicate message detected (idempotent skip)");
            return;
        }
    }
}