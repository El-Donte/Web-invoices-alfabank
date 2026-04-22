using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AbsIntegrationService.Models;
using Microsoft.EntityFrameworkCore;

namespace AbsIntegrationService.Infrastructure.Entities;

[Table("invoice_draft_lines")]
public class InvoiceDraftLineEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("invoice_draft_id")]
    public Guid InvoiceDraftId { get; set; }

    [ForeignKey(nameof(InvoiceDraftId))]
    public InvoiceDraft InvoiceDraft { get; set; } = null!;

    [Column("service_code")]
    [MaxLength(50)]
    public string ServiceCode { get; set; } = string.Empty;

    [Column("service_name")]
    [MaxLength(1000)]
    public string ServiceName { get; set; } = string.Empty;

    [Column("quantity")]
    [Precision(18, 4)]
    public decimal Quantity { get; set; } = 1m;

    [Column("unit")]
    [MaxLength(20)]
    public string Unit { get; set; } = "шт";

    [Column("price_without_nds")]
    [Precision(18, 2)]
    public decimal PriceWithoutNds { get; set; }

    [Column("nds_rate")]
    [Precision(5, 2)]
    public decimal NdsRate { get; set; } = 20m;

    [Column("amount_without_nds")]
    [Precision(18, 2)]
    public decimal AmountWithoutNds { get; set; }

    [Column("nds_amount")]
    [Precision(18, 2)]
    public decimal NdsAmount { get; set; }

    [Column("amount_with_Nds")]
    [Precision(18, 2)]
    public decimal AmountWithNds { get; set; }

    [Column("contract_number")]
    [MaxLength(100)]
    public string ContractNumber { get; set; } = string.Empty;
}