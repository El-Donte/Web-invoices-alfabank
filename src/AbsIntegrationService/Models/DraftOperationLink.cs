namespace AbsIntegrationService.Models;

public class DraftOperationLink
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceDraftId { get; set; }

    public string OperationNumber { get; set; } = string.Empty;
    public DateTime OperationDate { get; set; }
    public decimal Amount { get; set; }

    public string SourceMessageId { get; set; } = string.Empty;
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
}