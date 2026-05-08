namespace Auditable;

public abstract class AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public abstract class AuditableUserEntity : AuditableEntity
{
    public Guid CreatedById { get; set; }
    public Guid? LastModifiedById { get; set; }
}