using Messaging.Kafka.Producer;
using Shared.Contracts.Events;

namespace AbsIntegrationService.Services.Kafka;

public class AggregationReadyEventProducer(IKafkaProducer<AggregationReadyEvent>  kafkaProducer) : IAggregationReadyEventProducer
{
    public async Task ProduceAggregationEventAsync(AggregationReadyEvent aggGroup, CancellationToken token)
    {
        await kafkaProducer.ProduceAsync(aggGroup, aggGroup.AggregationGroupId.ToString(), token);
    }
}

public interface IAggregationReadyEventProducer
{
    public Task ProduceAggregationEventAsync(AggregationReadyEvent draftEvent, CancellationToken token);
}