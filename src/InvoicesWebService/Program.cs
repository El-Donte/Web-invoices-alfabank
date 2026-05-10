using InvoicesWebService.Infrastructure.Data;
using InvoicesWebService.Infrastructure.Repositories;
using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using InvoicesWebService.Services.DraftServices;
using InvoicesWebService.Services.Interfaces;
using InvoicesWebService.Services.Kafka;
using Messaging.Kafka;
using Messaging.Kafka.Producer;
using Npgsql;
using Shared;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

//db
builder.Services.AddSingleton<AuditInterceptor>();
builder.Services.AddDbContext<AppDbContext>();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

//repo
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IDraftInvoiceRepository, DraftInvoiceRepository>();
builder.Services.AddScoped<IRawTransactionReader, RawTransactionReader>();

//services
builder.Services.AddScoped<IProcessingErrorService, ProcessingErrorService>();
builder.Services.AddScoped<IDraftInvoiceService, DraftInvoiceService>();

//kafka
builder.Services.AddProducer<InvoiceTestCreatedMessage>(builder.Configuration.GetSection("Kafka:Abs"));
builder.Services.AddConsumer<AggregationReadyEvent, AggregationReadyConsumer>
    (builder.Configuration.GetSection("Kafka:AggregationGroup"));

//controllers
builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("InvoicesWebService"))
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
                o.Endpoint = new Uri("http://localhost:4318");
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            }));

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create-invoice", async (IKafkaProducer<InvoiceTestCreatedMessage> kafkaProducer, CancellationToken token) =>
{
    var test = new InvoiceTestCreatedMessage();
    await kafkaProducer.ProduceAsync(test, test.OperationNumber ,token);
});

app.UseRouting();
app.MapControllers();
app.Run();