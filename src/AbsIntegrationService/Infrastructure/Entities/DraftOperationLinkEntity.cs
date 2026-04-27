using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AbsIntegrationService.Models;
using Microsoft.EntityFrameworkCore;

namespace AbsIntegrationService.Infrastructure.Entities;

[Table("draft_operation_links")]
public class DraftOperationLinkEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("invoice_draft_id")]
    public Guid InvoiceDraftId { get; set; }

    [ForeignKey(nameof(InvoiceDraftId))]
    public InvoiceDraftEntity InvoiceDraft { get; set; } = null!;

    [Column("operation_number")]
    [MaxLength(100)]
    public string OperationNumber { get; set; } = string.Empty;

    [Column("operation_date")]
    public DateTime OperationDate { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("source_message_id")]
    [MaxLength(100)]
    public string SourceMessageId { get; set; } = string.Empty;

    [Column("linked_at")]
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
}