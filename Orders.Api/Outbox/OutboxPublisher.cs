using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Contracts;
using Orders.Api.Data;

namespace Orders.Api.Outbox;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisher> _logger;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishPendingMessages(stoppingToken);

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }

    private async Task PublishPendingMessages(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var publishEndpoint = scope.ServiceProvider
            .GetRequiredService<IPublishEndpoint>();

        var messages = await db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                if (message.Type == typeof(OrderCreated).FullName)
                {
                    var eventMessage =
                        JsonSerializer.Deserialize<OrderCreated>(
                            message.Payload);

                    if (eventMessage is null)
                    {
                        throw new InvalidOperationException(
                            $"Unable to deserialize OutboxMessage {message.Id}");
                    }

                    await publishEndpoint.Publish(
                        eventMessage,
                        context =>
                        {
                            context.CorrelationId =
                                eventMessage.CorrelationId;
                        },
                        cancellationToken);

                    message.ProcessedOnUtc = DateTime.UtcNow;

                    await db.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Published OutboxMessage {MessageId} of type {MessageType}",
                        message.Id,
                        message.Type);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish OutboxMessage {MessageId}",
                    message.Id);
            }
        }
    }
}
