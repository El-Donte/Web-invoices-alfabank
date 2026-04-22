using AbsIntegrationService.Models.Enums;

namespace AbsIntegrationService.Models;

public class InvoiceDraft(
    string operationNumber,
    DateTime operationDate,
    string sellerInn,
    string sellerKpp,
    string sellerName,
    string sellerAddress,
    string buyerInn,
    string buyerKpp,
    string buyerName,
    string buyerAddress,
    string currencyCode)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string OperationNumber { get; set; } = operationNumber;

    public DateTime OperationDate { get; set; } = operationDate;

    public InvoiceType Type { get; set; } = InvoiceType.Shipment;
    public DraftStatus Status { get; set; } = DraftStatus.New;
    
    public string SellerInn { get; set; } = sellerInn;
    public string SellerKpp { get; set; } = sellerKpp;
    public string SellerName { get; set; } = sellerName;
    public string SellerAddress { get; set; } = sellerAddress;

    public string BuyerInn { get; set; } = buyerInn;
    public string BuyerKpp { get; set; } = buyerKpp;
    public string BuyerName { get; set; } = buyerName;
    public string BuyerAddress { get; set; } = buyerAddress;

    public decimal TotalWithoutNds { get; set; }
    public decimal TotalNds { get; set; }
    public decimal TotalWithNds { get; set; }

    public string CurrencyCode { get; set; } = currencyCode;

    public List<InvoiceDraftLine> Lines { get; set; } = [];
    
    public List<DraftOperationLink> LinkedOperations { get; set; } = [];
    
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}