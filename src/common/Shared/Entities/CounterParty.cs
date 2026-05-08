using Auditable;

namespace Shared.Entities;

public class Counterparty :  AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Inn { get; set; } = string.Empty;
    public string Kpp { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}