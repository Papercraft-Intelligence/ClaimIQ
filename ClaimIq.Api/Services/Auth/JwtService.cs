using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClaimIq.Api.Services;

public class JwtService : IJwtService
{
    private readonly string _secretKey;

    public JwtService(IConfiguration configuration)
    {
        _secretKey = configuration["JWT_SECRET"] ?? "your-super-secret-key-for-demo-purposes-only-change-in-production";
    }

    public string GenerateToken(string tenantId, string userId, string userName)
    {
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", userId),
                new Claim("name", userName),
                new Claim("tenant_id", tenantId),  // 🔥 TENANT CLAIM FOR SECURITY
                new Claim("role", "user"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            }),
            Expires = DateTime.UtcNow.AddHours(8), // 8 hour token
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}