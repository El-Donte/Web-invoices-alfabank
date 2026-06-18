using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace InvoicesWebService.Services.Authorization;

public class JwtConfiguration
{
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public int ExpireMinutes { get; set; }
    public SymmetricSecurityKey SecretKey {get; set;}
}