using InvoicesAlfa.Services.Kafka;
using Messaging.Kafka;
using Messaging.Kafka.Producer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProducer<InvoiceTestCreatedMessage>(builder.Configuration.GetSection("Kafka:Abs"));

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