using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Entities;

public class AggregationGroup : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string OperationNumber { get; set; } = string.Empty;
    public DateTime? TransactionDate { get; set; }
    public AggregationStatus Status { get; set; }
    public int ShipmentCount { get; set; }
    public int AdvanceCount { get; set; }
    public int CorrectiveCount { get; set; }
    public int TotalCount { get; set; }
    public string ValidationError { get; set; } = string.Empty;
    public DateTime? LastProcessedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public Guid? CounterpartyId { get; set; }
    public Guid? DepartmentId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
}

public enum AggregationStatus
{
    [Column("Открыта")]
    Open = 0, 
    
    [Column("Готова")]
    Ready = 1, 
    
    [Column("Черновик")]
    Draft = 2, 
    
    [Column("Ошибка валидации")]
    ValidationError = 3, 
    
    [Column("Создан")]
    Created = 4
}