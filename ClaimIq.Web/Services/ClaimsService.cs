using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using ClaimIq.Domain.Models;

namespace ClaimIq.Web.Services;

public class ClaimsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private string _claimsEndpoint => $"{_apiBaseUrl}/claims";
    private readonly ILogger<ClaimsService> _logger;

    public ClaimsService(HttpClient httpClient, IConfiguration configuration, ILogger<ClaimsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:5234";
        
        _logger.LogInformation("🔧 ClaimsService initialized with API: {ApiUrl}", _apiBaseUrl);
    }

    public async Task<List<InsuranceClaim>> GetClaimsAsync()
    {
        try
        {
            _logger.LogInformation("🚀 Making HTTP request to: {Endpoint}", _claimsEndpoint);
            
            var response = await _httpClient.GetFromJsonAsync<List<InsuranceClaim>>(_claimsEndpoint);
            
            var count = response?.Count ?? 0;
            _logger.LogInformation("✅ SUCCESS! Received {Count} claims from API", count);
            
            return response ?? new List<InsuranceClaim>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error calling {Endpoint}: {Message}", _claimsEndpoint, ex.Message);
            return new List<InsuranceClaim>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Unexpected error fetching claims from {Endpoint}", _claimsEndpoint);
            return new List<InsuranceClaim>();
        }
    }
}