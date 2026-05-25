namespace Shared.Entities;

public class Department : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<DepartmentAccess> DepartmentAccesses { get; set; } = new List<DepartmentAccess>();
}