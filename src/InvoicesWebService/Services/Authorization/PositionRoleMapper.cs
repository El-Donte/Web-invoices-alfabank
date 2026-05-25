using Shared.Entities;

namespace InvoicesWebService.Services.Authorization;

public static class PositionRoleMapper
{
    public static string ToRole(Position position) => position switch
    {
        Position.Admin       => "Admin",
        Position.Factoring   => "Factoring",
        Position.Accounting  => "Accountant",
        Position.Taxation    => "Taxation",
        Position.Acquiring   => "Acquiring",
        _                    => "Accountant"
    };

    public static Position ToPosition(string role) => role.ToUpper() switch
    {
        "ADMIN"      => Position.Admin,
        "FACTORING"  => Position.Factoring,
        "ACCOUNTANT" => Position.Accounting,
        "TAXATION"   => Position.Taxation,
        "ACQUIRING"  => Position.Acquiring,
        _            => Position.Accounting
    };

    public static IEnumerable<string> GetAllRoles() =>
        Enum.GetValues<Position>().Select(ToRole);
}