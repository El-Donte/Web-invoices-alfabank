using System.Text.Json;
using Confluent.Kafka;

namespace Messaging.Kafka.Consumer;

public class KafkaJsonDeserializer<TMessage> : IDeserializer<TMessage>
{
    private readonly JsonSerializerOptions _serializeOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public TMessage Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<TMessage>(data, _serializeOptions)!;
    }
}