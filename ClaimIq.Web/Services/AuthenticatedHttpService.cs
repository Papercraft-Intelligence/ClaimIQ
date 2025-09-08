// ClaimIq.Web/Services/AuthenticatedHttpService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace ClaimIq.Web.Services;

public interface IAuthenticatedHttpService
{
    Task<T?> GetAsync<T>(global::System.String endpoint) where T : class;
    Task<HttpResponseMessage> PostAsync<T>(global::System.String endpoint, T data);
}

public class AuthenticatedHttpService : IAuthenticatedHttpService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AuthenticatedHttpService> _logger;

    public AuthenticatedHttpService(HttpClient httpClient, IJSRuntime jsRuntime, ILogger<AuthenticatedHttpService> logger)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string endpoint) where T : class
    {
        await AddAuthHeaderAsync();

        try
        {
            var response = await _httpClient.GetFromJsonAsync<T>(endpoint);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GET {Endpoint}", endpoint);
            return null;
        }
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        await AddAuthHeaderAsync();
        return await _httpClient.PostAsJsonAsync(endpoint, data);
    }

    private async Task AddAuthHeaderAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("🔐 Bearer token added to request");
            }
            else
            {
                _logger.LogWarning("⚠️ No auth token found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auth token");
        }
    }
}