using Microsoft.AspNetCore.Authorization;

namespace InvoicesWebService.Services.Authorization;

public class PermissionAuthorizationRequirement(params string[] permissions)
: AuthorizationHandler<PermissionAuthorizationRequirement>, IAuthorizationRequirement
{
    public string[] AllowedPermissions { get; } = permissions;
    
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
    {
        foreach (var permission in requirement.AllowedPermissions)
        {
            bool found = context.User.FindFirst(c =>
                c.Type == CustomClaim.Permission &&
                c.Value == permission) is not null;

            if (found)
            {
                context.Succeed(requirement);
                break;
            }
        }
        return Task.CompletedTask;
    }
}

public static class PermissionExtensions
{
    public static void RegisterPermissions(
        this AuthorizationPolicyBuilder builder,
        params string[] permissions)
    {
        builder.AddRequirements(new PermissionAuthorizationRequirement(permissions));
    }
}