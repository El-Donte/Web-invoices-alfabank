using System.Security.Claims;
using InvoicesWebService.Infrastructure.Data;
using InvoicesWebService.Models.Requests;
using InvoicesWebService.Models.Responses;
using InvoicesWebService.Services.Authorization;
using InvoicesWebService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shared.Entities;
using Shared.Results;

namespace InvoicesWebService.Services;

public class UserService(UserManager<User> userManager, JwtConfiguration configuration, UserDbContext dbContext) : IUserService
{
    public async Task<Result<UserResponse>> Login(UserLoginRequest request, CancellationToken ct)
    {
        var user = await userManager.FindByNameAsync(request.Login);

        if (user is null ||
            !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Errors.UserErrors.IncorrectPassword();
        }

        var roles = await userManager.GetRolesAsync(user);

        var singingKey = configuration.SecretKey;
        
        var credentials = new SigningCredentials(singingKey, SecurityAlgorithms.HmacSha256);

        var permissions = await (
                from role in dbContext.Roles
                join claim in dbContext.RoleClaims on role.Id equals claim.RoleId
                where roles.Contains(role.Name!) &&
                    claim.ClaimType == CustomClaim.Permission
                select claim.ClaimValue)
            .Distinct()
            .ToArrayAsync(cancellationToken: ct);
        
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.UserName),
            ..roles.Select(r => new Claim(ClaimTypes.Role, r)),
            ..permissions.Select(p => new Claim(CustomClaim.Permission, p))
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(configuration.ExpireMinutes),
            SigningCredentials = credentials,
            Issuer = configuration.Issuer,
            Audience = configuration.Audience
        };

        var tokenHandler = new JsonWebTokenHandler();

        var accessToken = tokenHandler.CreateToken(tokenDescriptor);

        return new UserResponse(accessToken, user);
    }

    public async Task<Result<Guid>> Register(UserRegisterRequest request, CancellationToken ct)
    {
        var user = new User
        {
            FullName = request.Name + " " + request.Surname,
            UserName = request.Login,
            Position = request.Position,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var identityResult = await userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            return Errors.General.ValuesIsInvalid(identityResult.Errors);
        }
        
        var addToRoleResult = await userManager.AddToRoleAsync(user, PositionRoleMapper.ToRole(request.Position));
        if (!addToRoleResult.Succeeded)
        {
            return Errors.General.ValuesIsInvalid(addToRoleResult.Errors);
        }
        
        return user.Id;
    }
}