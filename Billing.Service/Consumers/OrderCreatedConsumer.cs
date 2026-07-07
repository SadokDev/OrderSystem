using Billing.Service.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Contracts;

namespace Billing.Service.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly BillingDbContext _db;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(BillingDbContext db, ILogger<OrderCreatedConsumer>  logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing Order {OrderId} with CorrelationId {CorrelationId}",
            message.OrderId,
            context.CorrelationId);

        var messageId = message.OrderId;

        // 1. Idempotence check (DB-level safety)
        var alreadyProcessed = await _db.ProcessedMessages
            .AnyAsync(x => x.OrderId == messageId);

        if (alreadyProcessed)
        {
            _logger.LogWarning(
                "Duplicate Order detected {OrderId} with CorrelationId {CorrelationId}",
                messageId,
                context.CorrelationId);
            return;
        }

        // 2. Persist idempotence marker
        _db.ProcessedMessages.Add(new ProcessedMessage
        {
            OrderId = messageId,
            ProcessedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Order {OrderId} processed successfully with CorrelationId {CorrelationId}",
            messageId,
            context.CorrelationId);
    }
}