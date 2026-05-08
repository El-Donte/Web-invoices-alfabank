using System.ComponentModel.DataAnnotations.Schema;
using Auditable;

namespace Shared.Entities;

public class User : AuditableEntity
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
    public Position Position { get; set; }
}

public enum Position
{
    [Column("Админ")]
    Admin = 0,
    
    [Column("Факторинг")]
    Factoring = 1,
    
    [Column("Бухгалтер")]
    Accounting = 2,
    
    [Column("Налоговое управление")]
    Taxation = 3,
    
    [Column("Экваринг")]
    Acquiring = 4
}