using InvoicesWebService.Infrastructure.Data;
using InvoicesWebService.Infrastructure.Repositories;
using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using InvoicesWebService.Services.Kafka;
using Messaging.Kafka;
using Messaging.Kafka.Producer;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

//db
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.EnableThreadSafetyChecks();
});

//repo
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

//services


//controllers
builder.Services.AddControllers();

//kafka
builder.Services.AddProducer<InvoiceTestCreatedMessage>(builder.Configuration.GetSection("Kafka:Abs"));
// builder.Services.AddConsumer<InvoiceDraft, InvoiceDraftMessageHandler>(builder.Configuration.GetSection("Kafka:InvoiceDraft"));

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