using Billing.Service.Consumers;
using Billing.Service.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? "";
var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? "";

var otelEndpoint = builder.Configuration["OpenTelemetry:Endpoint"]
                   ?? "http://localhost:4318/v1/traces";

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
        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUsername);
            h.Password(rabbitMqPassword);
        });

        cfg.UseMessageRetry(r =>
        {
            r.Exponential(
                retryLimit: 5,
                minInterval: TimeSpan.FromSeconds(1),
                maxInterval: TimeSpan.FromSeconds(30),
                intervalDelta: TimeSpan.FromSeconds(5));
        });

        cfg.ReceiveEndpoint("billing-service", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });
    });
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
    {
        resource.AddService("billing-service");
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("MassTransit")
            .AddConsoleExporter()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otelEndpoint);
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
                options.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
            });
    });

builder.Services.AddMassTransitHostedService();

var host = builder.Build();
host.Run();