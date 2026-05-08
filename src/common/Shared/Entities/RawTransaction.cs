using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Auditable;

namespace Shared.Entities;

public class RawTransaction : AuditableEntity
{
    public string OperationNumber { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public DateTime? Date { get; set; }
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
}

public enum TransactionStatus
{
    [Column("Получен")]
    Received = 0, 
    
    [Column("Ошибка валидации")]
    ValidationError = 1, 
    
    [Column("Обработан")]
    Processed = 2
}

public enum TransactionType
{
    [Display(Name = "Отгрузочный")]
    Shipment = 0,

    [Display(Name = "Авансовый")]
    Advance = 1,

    [Display(Name = "Корректировочный")]
    Corrective = 2
}