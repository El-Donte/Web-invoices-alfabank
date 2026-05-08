using System.Text.Json;

namespace AbsIntegrationService.Services.Kafka;

public class JsonDeserializer<TMessage>
{
    private readonly JsonSerializerOptions _serializeOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public TMessage Deserialize(string data)
    {
        return JsonSerializer.Deserialize<TMessage>(data, _serializeOptions)!;
    }
}