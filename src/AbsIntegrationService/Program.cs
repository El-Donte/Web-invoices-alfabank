using AbsIntegrationService.Infrastructure.Data;
using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Services;
using AbsIntegrationService.Services.Aggregation;
using AbsIntegrationService.Services.Interfaces;
using AbsIntegrationService.Services.Kafka;
using AbsIntegrationService.Workers;
using Messaging.Kafka;
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

var app = builder.Build();

app.Run();