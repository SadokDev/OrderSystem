using Billing.Service.Consumers;
using Billing.Service.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<BillingDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("BillingDb"));
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // 🔥 RETRY POLICY HERE
        cfg.UseMessageRetry(r =>
        {
            r.Interval(3, TimeSpan.FromSeconds(2));
        });

        cfg.ReceiveEndpoint("billing-service", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });
    });
});

builder.Services.AddMassTransitHostedService();

var host = builder.Build();
host.Run();