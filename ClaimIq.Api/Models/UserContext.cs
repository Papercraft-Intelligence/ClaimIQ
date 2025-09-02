namespace ClaimIq.Api.Models;

public class UserContext
{
    public string UserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Environment { get; set; } = "production";

    public Dictionary<string, string> Attributes { get; set; } = new();

    public string? Segment => Attributes.GetValueOrDefault("segment")?.ToString();
    public string? Country => Attributes.GetValueOrDefault("country")?.ToString();
    public string? Email => Attributes.GetValueOrDefault("email")?.ToString();
    public string? DeviceType => Attributes.GetValueOrDefault("deviceType")?.ToString();

    public int GetUserHash(string flagKey)
    {
        return Math.Abs((UserId + flagKey).GetHashCode());
    }
}