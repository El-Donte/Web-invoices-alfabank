using System.Threading.Channels;
using AbsIntegrationService.Infrastructure.Data;
using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Services;
using AbsIntegrationService.Services.Aggregation;
using AbsIntegrationService.Services.Interfaces;
using AbsIntegrationService.Services.Kafka;
using AbsIntegrationService.Workers;
using Messaging.Kafka;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Shared;
using Shared.Contracts;
using Shared.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

//db
builder.Services.AddSingleton<AuditInterceptor>();
builder.Services.AddSingleton<EfCoreMetricsInterceptor>();
builder.Services.AddDbContextFactory<AppDbContext>();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

//repos
builder.Services.AddScoped<ICounterpartyRepository, CounterpartyRepository>();
builder.Services.AddScoped<IAggregationRepository, AggregationRepository>();
builder.Services.AddScoped<IProcessingErrorService, ProcessingErrorService>();
builder.Services.AddScoped<IRawTransactionRepository, RawTransactionRepository>();

//services
builder.Services.AddSingleton<ITransactionIngestionService, TransactionIngestionService>();
builder.Services.AddScoped<IValidationService, ValidationService>();

builder.Services.Configure<AggregationWorkerSettings>(builder.Configuration.GetSection("AggregationWorker"));
builder.Services.AddSingleton<IAggregationService, AggregationService>();
builder.Services.AddHostedService<AggregationScheduledWorker>();

var eventChannel = Channel.CreateBounded<AggregationReadyEvent>(new BoundedChannelOptions(10_000)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = true,
    SingleWriter = false
});
builder.Services.AddSingleton(eventChannel);
builder.Services.AddHostedService<EventPublisherWorker>();

//kafka
builder.Services.AddProducer<AggregationReadyEvent>(builder.Configuration.GetSection("Kafka:AggregationGroup"));
builder.Services.AddSingleton<IAggregationReadyEventProducer, AggregationReadyEventProducer>();
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
        .AddMeter("AbsIntegrationService")
        .AddPrometheusExporter())
    .WithTracing(tracing =>
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddNpgsql()
            .AddSource("AbsIntegrationService")
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://tempo:4318");
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            }));

builder.Services.AddControllers();

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseRouting();
app.MapMetrics();
app.MapControllers();
app.Run();
