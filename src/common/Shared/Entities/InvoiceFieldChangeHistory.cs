namespace Shared.Entities;

public class InvoiceFieldChangeHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FieldName { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid InvoiceId { get; set; }
    public Guid? ChangedById { get; set; }
}