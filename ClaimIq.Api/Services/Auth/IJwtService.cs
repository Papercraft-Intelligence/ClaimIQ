namespace ClaimIq.Api.Services;

public interface IJwtService
{
    string GenerateToken(string tenantId, string userId, string userName);
}