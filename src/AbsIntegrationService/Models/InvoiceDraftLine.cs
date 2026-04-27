namespace AbsIntegrationService.Models;

public class InvoiceDraftLine(
    Guid invoiceDraftId,
    string serviceCode,
    string serviceName,
    decimal quantity,
    string unit,
    decimal ndsRate,
    decimal priceWithoutNds,
    string contractNumber)
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InvoiceDraftId { get; set; } =  invoiceDraftId;
    
    public string ServiceCode { get; set; } = serviceCode;
    public string ServiceName { get; set; } = serviceName;

    public decimal Quantity { get; set; } = quantity;
    public string Unit { get; set; } = unit;

    public decimal PriceWithoutNds { get; set; } =  priceWithoutNds;
    public decimal NdsRate { get; set; } = ndsRate;
    public decimal AmountWithoutNds { get; set; }
    public decimal NdsAmount { get; set; }
    public decimal AmountWithNds { get; set; }
    
    public string ContractNumber { get; set; } = contractNumber;
}