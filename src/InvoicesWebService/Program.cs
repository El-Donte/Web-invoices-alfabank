using InvoicesWebService.Infrastructure.Data;
using InvoicesWebService.Infrastructure.Repositories;
using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using InvoicesWebService.Services.DraftServices;
using InvoicesWebService.Services.Interfaces;
using InvoicesWebService.Services.Kafka;
using Messaging.Kafka;
using Messaging.Kafka.Producer;
using Shared;
using Shared.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

//db
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
builder.Services.AddProducer<InvoiceTestCreatedMessage>(builder.Configuration.GetSection("Kafka:AbsMessage"));
builder.Services.AddConsumer<AggregationReadyEvent, AggregationReadyConsumer>
    (builder.Configuration.GetSection("Kafka:AggregationGroup"));

//controllers
builder.Services.AddControllers();


builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.Run();