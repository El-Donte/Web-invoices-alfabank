using Microsoft.AspNetCore.Identity;

namespace Shared.Entities;

public class User : IdentityUser<Guid>, IAuditableEntity
{
    public override Guid Id { get; set; } =  Guid.NewGuid();
    [PersonalData]
    public string FullName { get; set; }
    public Position Position { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<DepartmentAccess> DepartmentAccesses { get; set; } = new List<DepartmentAccess>();
}

public enum Position
{
    Admin = 0,
    Factoring = 1,
    Accounting = 2,
    Taxation = 3,
    Acquiring = 4
}