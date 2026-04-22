using AbsIntegrationService.Models;

namespace AbsIntegrationService.Contracts;

public class InvoiceDraftCreatedEvent
{
    public Guid DraftId  { get; set; }
    public string OperationNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string BuyerInn { get; set; }
    public string BuyerName { get; set; }
    public decimal TotalWithNds {get; set;}
    public List<InvoiceDraftLine> Lines { get; set; }
    public int LinkedOperationsCount { get; set; }
}