using System.ComponentModel.DataAnnotations.Schema;
using Shared;

namespace Shared.Entities;

public class ProcessingError : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ProcessingStage Stage { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public bool Retryable { get; set; } = true;
    public bool Resolved { get; set; }
    public Guid? AggregationGroupId { get; set; }
    public Guid? DraftInvoiceId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? RawTransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum ProcessingStage
{
     Ingest = 0,
     Proceeding = 1,
     Aggregation = 2,
     Creation = 3,
     Export = 4
}