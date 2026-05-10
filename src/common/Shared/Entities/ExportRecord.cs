using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Entities;

public class ExportRecord : IAuditableEntity
{
    public Guid Id { get; set; } =  Guid.NewGuid();
    public ExportStatus Status { get; set; }
    public string Destination { get; set; } = "csv";
    public string LastError { get; set; } = string.Empty;
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? ExportedAt { get; set; }
    public Guid InvoiceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum ExportStatus
{
    [Column("Готов")]
    Ready = 0, 
    
    [Column("Отправлен")]
    Sent = 1, 
    
    [Column("Ошибка")]
    Error = 2, 
    
    [Column("Повторная отправка")]
    RetrySending = 3
}