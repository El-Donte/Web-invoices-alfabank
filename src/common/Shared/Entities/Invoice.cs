using System.ComponentModel.DataAnnotations;
using Shared;

namespace Shared.Entities;

public class Invoice : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Number { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public decimal NdsRate { get; set; }
    public decimal TotalNds { get; set; }
    public decimal TotalWithNds { get; set; }
    public decimal TotalWithoutNds { get; set; }
    public string CurrencyCode { get; set; } = "RUB";
    public InvoiceStatus Status { get; set; }
    public DateTime? PaymentDocDate { get; set; }
    public string PaymentDocNumber { get; set; } = string.Empty;
    public string SequenceNumber { get; set; } = string.Empty;
    public int CurrentVersion { get; set; } = 1;
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid? DraftInvoiceId { get; set; }
    public Guid? LastAuthorId { get; set; }
    
    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum InvoiceType
{
    [Display(Name = "Отгрузочный")]
    Shipment = 0,

    [Display(Name = "Авансовый")]
    Advance = 1,

    [Display(Name = "Корректировочный")]
    Corrective = 2
}

public enum InvoiceStatus
{
    [Display(Name = "Создан")]
    Created = 0,

    [Display(Name = "Ошибка отправки")]
    SendingError = 1,

    [Display(Name = "Отправить")]
    Sent = 2,
}