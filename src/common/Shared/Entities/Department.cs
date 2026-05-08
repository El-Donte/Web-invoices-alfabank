using Auditable;

namespace Shared.Entities;

public class Department : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}