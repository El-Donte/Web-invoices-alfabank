using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using LoadGenerator;


public class KafkaJsonSerializer<TMessage> : ISerializer<TMessage>
{
    private readonly JsonSerializerOptions _serializeOptions =  new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public byte[] Serialize(TMessage data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data, _serializeOptions);
    }
}

internal class Program
{
    private static string adsad = "localhost:9094";
    static ProducerConfig config = new()
    {
        BootstrapServers = adsad,
        Acks = Acks.All,
        MessageSendMaxRetries = 5,
        RetryBackoffMs = 500,
        EnableIdempotence = true,
        LingerMs = 20,
        CompressionType = CompressionType.Lz4,
        BatchNumMessages = 200_000,
        BatchSize = 16_777_216,
        MessageMaxBytes = 25_000_000,
        QueueBufferingMaxMessages = 5_000_000,
        QueueBufferingMaxKbytes = 1_048_576,
        RequestTimeoutMs = 60_000,
        SocketTimeoutMs = 60_000,
    };
    
    static string[] departmentIds = 
    [
        "c367d653-4d63-482e-bc5e-3a253baeabcf",
        "145b5db8-049a-486f-95ba-b2402bc5e844",
        "e4803427-ae14-4c3a-ae57-4792caf4342c",
        "cc46df69-71ea-4a44-b5cf-13e11cad2327"
    ];

    static string[] counterpartyIds = [
        "7707083893", 
        "7707083894", 
        "6607083894", 
        "6607083893", 
        "5507083893" 
    ];
    
    static AbsMessage GenerateMessage()
    {
        var random = Random.Shared;
        var ndsRate = 0.20m;
        var quantity = random.Next(1, 11);
        var unitPrice = random.Next(100, 5001);
        var amount = quantity * unitPrice;
        var ndsAmount = Math.Round(amount * ndsRate, 2);


        return new AbsMessage
        {
            MessageId = Guid.NewGuid(),
            OperationNumber = $"OP-{Guid.NewGuid()}",
            Type = (TransactionType)random.Next(0, 3),
            OperationDate = DateTime.UtcNow,

            UnitPrice = unitPrice,
            NdsRate = ndsRate,
            NdsAmount = ndsAmount,
            Amount = amount,
            CurrencyCode = "RUB",

            BuyerInn = counterpartyIds[random.Next(counterpartyIds.Length)],
            BuyerKpp = "770701001",
            BuyerName = "Тестовый контрагент",
            BuyerAddress = "г. Москва, ул. Тестовая, д.1",

            SellerInn = "7707083893",
            SellerKpp = "770701001",
            SellerName = "Наша компания",
            SellerAddress = "г. Москва",

            ProductName = "Услуга эквайринга / факторинга",
            ProductCode = $"PRD-{random.Next(10000, 99999)}",
            Unit = "шт",
            Quantity = quantity,

            ContractNumber = $"CTR-{random.Next(100000, 999999)}",
            ContractDate = DateTime.UtcNow,
            PaymentDocumentNumber = $"PD-{random.Next(1000000, 9999999)}",
            OperationType = "PAYMENT",

            DepartmentId = Guid.Parse(departmentIds[random.Next(departmentIds.Length)]),
            PostedAt = DateTime.UtcNow,
        };
    }
    
    public static async Task Main(string[] args)
    {
        const string Topic = "abs.message";
        const int TotalMessages = 10_000_000;
        
        using var producer = new ProducerBuilder<string, AbsMessage>(config)
            .SetValueSerializer(new KafkaJsonSerializer<AbsMessage>())
            .Build();

        var sw = Stopwatch.StartNew();
        var sent = 0;
        var batchSize = 25_000;

        Console.WriteLine($"Starting load test → {TotalMessages:N0} messages to topic '{Topic}'");

        try
        {
            for (int i = 0; i < TotalMessages; i += batchSize)
            {
                var currentBatch = Math.Min(batchSize, TotalMessages - i);
                var tasks = new List<Task>(currentBatch);

                for (int j = 0; j < currentBatch; j++)
                {
                    var transaction = GenerateMessage();
                    var key = $"load-{i+j}-{Guid.NewGuid():N}";

                    tasks.Add(producer.ProduceAsync(Topic, new Message<string, AbsMessage>
                    {
                        Key = key,
                        Value = transaction
                    }));
                }

                await Task.WhenAll(tasks);
                sent += currentBatch;

                if (sent % 500_000 == 0)
                {
                    var elapsedSec = sw.Elapsed.TotalSeconds;
                    var rate = sent / elapsedSec;
                    Console.WriteLine($"Sent: {sent:N0} | Rate: {rate:F0} msg/sec | Elapsed: {elapsedSec:F1}s");
                }
            }

            producer.Flush(TimeSpan.FromSeconds(60));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            var totalSec = sw.Elapsed.TotalSeconds;
            Console.WriteLine($"\n=== FINISHED ===");
            Console.WriteLine($"Total messages: {sent:N0}");
            Console.WriteLine($"Total time: {totalSec:F2} sec");
            Console.WriteLine($"Average speed: {sent / totalSec:F0} messages/sec");
        }

        Console.ReadLine();
    }
}