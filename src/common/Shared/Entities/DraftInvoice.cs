using System.ComponentModel.DataAnnotations;

namespace Shared.Entities;

public class DraftInvoice : IAuditableEntity
{ 
    public Guid Id { get; set; } =  Guid.NewGuid();
    public DateTime TransactionDate { get; set; }
    public decimal NdsRate { get; set; }
    public decimal TotalNdsAmount { get; set; }
    public decimal TotalWithNds { get; set; }

    public decimal TotalWithoutNds
    {
        get =>TotalWithNds - TotalNdsAmount;
        set;
    }
    
    public string CurrencyCode { get; set; } = "RUB";
    public DraftInvoiceStatus Status { get; set; }
    public string ValidationError { get; set; } = string.Empty;
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid AggregationGroupId { get; set; }
    public ICollection<DraftInvoiceLine> Lines { get; set; } = new List<DraftInvoiceLine>();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum DraftInvoiceStatus
{
    [Display(Name = "Новый")]
    New = 0,

    [Display(Name = "Ошибка валидации")]
    ValidatingError  = 1
}