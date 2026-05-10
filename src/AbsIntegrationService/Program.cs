using AbsIntegrationService.Infrastructure.Data;
using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Services;
using AbsIntegrationService.Services.Aggregation;
using AbsIntegrationService.Services.Interfaces;
using AbsIntegrationService.Services.Kafka;
using AbsIntegrationService.Workers;
using OpenTelemetry;
using Messaging.Kafka;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared;
using Shared.Contracts;
using Shared.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

//db
builder.Services.AddDbContext<AppDbContext>();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

//repos
builder.Services.AddScoped<IAggregationRepository, AggregationRepository>();
builder.Services.AddScoped<IProcessingErrorService, ProcessingErrorService>();
builder.Services.AddScoped<IRawTransactionRepository, RawTransactionRepository>();

//services
builder.Services.AddScoped<ITransactionIngestionService, TransactionIngestionService>();
builder.Services.AddScoped<IValidationService, ValidationService>();

builder.Services.Configure<AggregationWorkerSettings>(builder.Configuration.GetSection("AggregationWorker"));
builder.Services.AddScoped<IAggregationService, AggregationService>();
builder.Services.AddHostedService<AggregationScheduledWorker>();

//kafka
builder.Services.AddProducer<AggregationReadyEvent>(builder.Configuration.GetSection("Kafka:AggregationGroup"));
builder.Services.AddScoped<IAggregationReadyEventProducer, AggregationReadyEventProducer>();
builder.Services.AddConsumer<AbsMessage, RawTransactionConsumer>(builder.Configuration.GetSection("Kafka:Abs"));

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("AbsIntegrationService"))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddNpgsqlInstrumentation()
        .AddMeter("InvoiceSystem")
        .AddPrometheusExporter())
    .WithTracing(tracing =>
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddNpgsql()
            .AddSource("InvoiceSystem")
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://tempo:4318");
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            }))
    .UseOtlpExporter();

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseRouting();
app.MapControllers();
app.Run();
