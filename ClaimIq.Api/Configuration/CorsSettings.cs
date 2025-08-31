namespace ClaimIq.Api.Configuration;

public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public bool AllowCredentials { get; set; } = false;
    public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
    public string[] AllowedHeaders { get; set; } = new[] { "*" };
    public string[] EnvironmentProvidedOrigins =>
        Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")?
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ?? Array.Empty<string>();
    public string[] ResolvedOrigins => AllowedOrigins
        .Concat(EnvironmentProvidedOrigins)
        .Distinct()
        .Where(o => !string.IsNullOrWhiteSpace(o))
        .ToArray();
}
