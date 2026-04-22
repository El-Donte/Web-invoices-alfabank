using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AbsIntegrationService.Models;
using Microsoft.EntityFrameworkCore;

namespace AbsIntegrationService.Infrastructure.Entities;

[Table("invoice_drafts")]
public class InvoiceDraftEntity
{ 
    [Key] 
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [Column("operation_number")]
    [MaxLength(100)]
    public string OperationNumber { get; set; } = string.Empty;

    [Column("operation_date")]
    public DateTime OperationDate { get; set; }

    [Column("type")]
    public InvoiceType Type { get; set; } = InvoiceType.Shipment;

    [Column("status")]
    public DraftStatus Status { get; set; } = DraftStatus.New;
    
    
    [Column("seller_inn")]
    [MaxLength(12)]
    public string SellerInn { get; set; } = string.Empty;

    [Column("seller_kpp")]
    [MaxLength(9)]
    public string SellerKpp { get; set; } = string.Empty;

    [Column("seller_name")]
    [MaxLength(500)]
    public string SellerName { get; set; } = string.Empty;
    
    [Column("seller_address")]
    [MaxLength(500)]
    public string SellerAddress { get; set; } = string.Empty;


    [Column("buyer_inn")]
    [MaxLength(12)]
    public string BuyerInn { get; set; } = string.Empty;

    [Column("buyer_kpp")]
    [MaxLength(9)]
    public string BuyerKpp { get; set; } = string.Empty;

    [Column("buyer_name")]
    [MaxLength(500)]
    public string BuyerName { get; set; } = string.Empty;

    [Column("buyer_address")]
    [MaxLength(1000)]
    public string BuyerAddress { get; set; } = string.Empty;


    
    [Column("total_without_nds")]
    [Precision(18, 2)]
    public decimal TotalWithoutNds { get; set; }

    [Column("total_nds")]
    [Precision(18, 2)]
    public decimal TotalNds { get; set; }

    [Column("total_with_nds")]
    [Precision(18, 2)]
    public decimal TotalWithNds { get; set; }

    [Column("currency_code")]
    [MaxLength(3)]
    public string CurrencyCode { get; set; } = "RUB";

    
    public ICollection<InvoiceDraftLineEntity> Lines { get; set; } = new List<InvoiceDraftLineEntity>();

    public ICollection<DraftOperationLinkEntity> LinkedOperations { get; set; } = new List<DraftOperationLinkEntity>();


    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }

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

public enum DraftStatus
{
    [Display(Name = "Новый")]
    New = 0,

    [Display(Name = "В обработке")]
    Processing = 1,

    [Display(Name = "Готов")]
    Ready = 2,

    [Display(Name = "Ошибка")]
    Error = 3,

    [Display(Name = "Игнорирован")]
    Ignored = 4
}