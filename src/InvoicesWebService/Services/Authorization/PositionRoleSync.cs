using Microsoft.AspNetCore.Identity;
using Shared.Entities;

namespace InvoicesWebService.Services.Authorization;

public interface IPositionRoleSyncService
{
    Task SyncPositionWithRolesAsync(User user, CancellationToken ct = default);
    Task SyncAllUsersAsync(CancellationToken ct = default);
}

public class PositionRoleSyncService(UserManager<User> userManager) : IPositionRoleSyncService
{
    public async Task SyncPositionWithRolesAsync(User user, CancellationToken ct = default)
    {
        if (user == null) return;

        var currentRoles = await userManager.GetRolesAsync(user);
        var targetRole = PositionRoleMapper.ToRole(user.Position);
        
        if (currentRoles.Any())
            await userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!await userManager.IsInRoleAsync(user, targetRole))
        {
            var result = await userManager.AddToRoleAsync(user, targetRole);
            if (!result.Succeeded)
            {
                return;
            }
        }
        //ToDo: results
    }

    public async Task SyncAllUsersAsync(CancellationToken ct = default)
    {
        var users = userManager.Users.ToList();
        foreach (var user in users)
        {
            await SyncPositionWithRolesAsync(user, ct);
        }
    }
}