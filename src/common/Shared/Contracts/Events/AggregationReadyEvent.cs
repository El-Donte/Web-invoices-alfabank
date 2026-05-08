
namespace Shared.Contracts.Events;

public record AggregationReadyEvent(
    Guid AggregationGroupId,
    string OperationNumber,
    DateTime? ReadyAt,
    int ShipmentCount,
    int AdvanceCount,
    int CorrectiveCount,
    int TotalCount);