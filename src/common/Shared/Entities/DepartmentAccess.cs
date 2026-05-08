namespace Shared.Entities;

public class DepartmentAccess
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DepartmentId { get; set; }
    public Guid UserId { get; set; }
}