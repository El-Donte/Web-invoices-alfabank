using AbsIntegrationService.Contracts;
using AbsIntegrationService.Infrastructure.Data;
using AbsIntegrationService.Infrastructure.MappingProfiles;
using AbsIntegrationService.Infrastructure.Repositories;
using AbsIntegrationService.Services.DraftCalculator;
using AbsIntegrationService.Services.Kafka;
using Messaging.Kafka;

var builder = WebApplication.CreateBuilder(args);

//db and auto mapper
builder.Services.AddAutoMapper(cfg => {
     cfg.AddMaps(typeof(InvoiceDraftMapperProfile).Assembly); 
     cfg.AddMaps(typeof(InvoiceDraftLineMapperProfile).Assembly); 
});
builder.Services.AddDbContext<AppDbContext>();

//repos
builder.Services.AddScoped<IInvoiceDraftRepository, InvoiceDraftRepository>();

//services 
builder.Services.AddScoped<IDraftCalculatorService, DraftCalculatorService>();

//kafka
builder.Services.AddProducer<InvoiceDraftCreatedEvent>(builder.Configuration.GetSection("Kafka:Invoices"));
builder.Services.AddScoped<IInvoiceDraftCreatedProducer, InvoiceDraftCreatedProducer>();
builder.Services.AddConsumer<AbsMessage, AbsMessageHandler>(builder.Configuration.GetSection("Kafka:Abs"));

var app = builder.Build();

app.Run();