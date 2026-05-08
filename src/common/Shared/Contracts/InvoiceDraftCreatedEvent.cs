using Shared.Entities;


namespace Shared.Contracts;

public class InvoiceDraftCreatedEvent
{
    public Guid DraftId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string SellerInn { get; set; }
    public string SellerKpp { get; set; } 
    public string SellerName { get; set; } 
    public string SellerAddress { get; set; } 

    public string BuyerInn { get; set; } 
    public string BuyerKpp { get; set; } 
    public string BuyerName { get; set; } 
    public string BuyerAddress { get; set; }

    public decimal TotalWithoutNds { get; set; }
    public decimal TotalNds { get; set; }
    public decimal TotalWithNds { get; set; }
    
    public string CurrencyCode { get; set; }

    public TransactionType Type { get; set; } = TransactionType.Shipment;
    
    public List<DraftInvoiceLine> Lines { get; set; }
    
    public string SourceSystem { get; set; } = "AbsIngestionService";
}