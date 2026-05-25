using Messaging.Kafka.Producer;
using Shared.Contracts.Events;

namespace AbsIntegrationService.Services.Kafka;

public class AggregationReadyEventProducer(IKafkaProducer<AggregationReadyEvent>  kafkaProducer) : IAggregationReadyEventProducer
{
    public async Task ProduceBatchAggregationEventAsync(IEnumerable<AggregationReadyEvent> aggGroups, CancellationToken token)
    {
        var tasks = aggGroups
            .Select(evt => 
                kafkaProducer.ProduceAsync(evt, evt.AggregationGroupId.ToString(), token))
            .ToList(); 

        await Task.WhenAll(tasks);
    }
}

public interface IAggregationReadyEventProducer
{
    public Task ProduceBatchAggregationEventAsync(IEnumerable<AggregationReadyEvent> draftEvents, CancellationToken token);
}