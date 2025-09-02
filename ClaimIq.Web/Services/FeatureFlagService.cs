using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using ClaimIq.Domain.Models;

namespace ClaimIq.Web.Services;

public class FeatureFlagService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private string _featureFlagEndpoint => $"{_apiBaseUrl}/FeatureFlag";
    private readonly ILogger<FeatureFlagService> _logger;

    public FeatureFlagService(HttpClient httpClient, IConfiguration configuration, ILogger<FeatureFlagService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:5234";
        
        _logger.LogInformation("🔧 FeatureFlagService initialized with API: {ApiUrl}", _apiBaseUrl);
    }

    public async Task<List<FeatureFlag>> GetAllFeatureFlagsAsync()
    {
        try
        {
            _logger.LogInformation("🚀 Making HTTP request to: {Endpoint}", _featureFlagEndpoint);
            
            var response = await _httpClient.GetFromJsonAsync<List<FeatureFlag>>(_featureFlagEndpoint);
            
            var count = response?.Count ?? 0;
            _logger.LogInformation("✅ SUCCESS! Received {Count} feature flags from API", count);
            
            return response ?? new List<FeatureFlag>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error calling {Endpoint}: {Message}", _featureFlagEndpoint, ex.Message);
            return new List<FeatureFlag>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Unexpected error fetching feature flags from {Endpoint}", _featureFlagEndpoint);
            return new List<FeatureFlag>();
        }
    }

    public async Task<FeatureFlagEvaluationResult?> EvaluateFeatureFlagAsync(string flagKey)
    {
        try
        {
            var endpoint = $"{_featureFlagEndpoint}/evaluate?flagKey={flagKey}";
            _logger.LogInformation("🚀 Making HTTP request to: {Endpoint}", endpoint);
            
            var response = await _httpClient.GetFromJsonAsync<FeatureFlagEvaluationResult>(endpoint);
            
            _logger.LogInformation("✅ SUCCESS! Evaluated flag {FlagKey} = {Enabled}", flagKey, response?.Enabled);
            
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error evaluating flag {FlagKey}: {Message}", flagKey, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Unexpected error evaluating flag {FlagKey}", flagKey);
            return null;
        }
    }
}