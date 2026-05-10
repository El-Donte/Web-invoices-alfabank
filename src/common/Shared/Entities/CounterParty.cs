namespace Shared.Entities;

public class Counterparty :  IAuditableEntity
{
    public Guid Id { get; set; } =  Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    public string Inn { get; set; } = string.Empty;
    public string Kpp { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}