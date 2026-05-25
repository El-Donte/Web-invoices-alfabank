using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace InvoicesWebService.Services.Authorization;

public class JwtConfiguration(IConfiguration configuration)
{
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public int ExpireMinutes { get; set; }
    public SymmetricSecurityKey SecretKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
}