using System.Text.Json;
using Confluent.Kafka;

namespace Messaging.Kafka.Producer;

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