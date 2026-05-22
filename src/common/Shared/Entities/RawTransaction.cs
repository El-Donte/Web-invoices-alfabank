using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared;

namespace Shared.Entities;

public class RawTransaction : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OperationNumber { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public TransactionType Type { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string UnitMeasure { get; set; } = "шт";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal NdsRate { get; set; }
    public decimal NdsAmount { get; set; }
    public decimal Amount { get; set; }
    public string PayloadHash { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public TransactionStatus Status { get; set; }
    public string ValidationError { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    public Guid? AggregationGroupId { get; set; }
    public Guid? CounterpartyId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? InvoiceId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum TransactionStatus
{
    Received = 0, 
    ValidationError = 1, 
    Processed = 2
}

public enum TransactionType
{
    Shipment = 0,
    Advance = 1,
    Corrective = 2
}