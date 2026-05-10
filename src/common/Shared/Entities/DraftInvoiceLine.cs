namespace Shared.Entities;

public class DraftInvoiceLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string Unit { get; set; } = "шт";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal AmountWithoutNds { get; set; }
    public decimal NdsRate { get; set; } = 20m;
    public decimal NdsAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid DraftInvoiceId { get; set; }
    public DraftInvoice DraftInvoice { get; set; }
    public Guid? RawTransactionId { get; set; }
}