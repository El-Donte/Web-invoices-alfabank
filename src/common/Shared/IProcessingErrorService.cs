using Shared.Entities;

namespace Shared;

public interface IProcessingErrorService
{
    Task LogAsync(ErrorLogEntry error, CancellationToken ct = default);
}

public record ErrorLogEntry(
    ProcessingStage Stage,
    string Code,
    string Message,
    string Payload,
    bool Retryable = true,
    Guid? AggregationGroupId = null,
    Guid? DraftInvoiceId = null,
    Guid? InvoiceId = null,
    Guid? RawTransactionId = null);