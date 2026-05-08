namespace Shared.Entities;

public class InvoiceLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal AmountWithoutNds { get; set; }
    public decimal AmountWithNds { get; set; }
    public decimal NdsRate { get; set; } = 20m;
    public decimal NdsAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; }
}