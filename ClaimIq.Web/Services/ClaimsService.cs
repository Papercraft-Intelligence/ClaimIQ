using ClaimIq.Domain.Models;
using ClaimIq.Web.Services;

namespace ClaimIq.Web.Services;
public class ClaimsService
{
    private readonly AuthenticatedHttpService _authHttp;
    private readonly string _apiBaseUrl;
    private string _claimsEndpoint => $"{_apiBaseUrl}/claims";
    private readonly ILogger<ClaimsService> _logger;

    public ClaimsService(AuthenticatedHttpService authHttp, IConfiguration configuration, ILogger<ClaimsService> logger)
    {
        _authHttp = authHttp;
        _logger = logger;
        _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:5234";
    }

    public async Task<List<InsuranceClaim>> GetClaimsAsync()
    {
        try
        {
            _logger.LogInformation("🚀 Getting claims from: {Endpoint}", _claimsEndpoint);
            
            var response = await _authHttp.GetAsync<List<InsuranceClaim>>(_claimsEndpoint);
            
            var count = response?.Count ?? 0;
            _logger.LogInformation("✅ SUCCESS! Received {Count} claims", count);
            
            return response ?? new List<InsuranceClaim>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error fetching claims");
            return new List<InsuranceClaim>();
        }
    }
}