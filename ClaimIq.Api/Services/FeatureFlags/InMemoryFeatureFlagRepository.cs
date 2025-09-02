using ClaimIq.Api.Models.FeatureFlags;

namespace ClaimIq.Api.Services.FeatureFlags;

public class InMemoryFeatureFlagRepository : IFeatureFlagRepository
{
    private static readonly Dictionary<string, FeatureFlag> _flags = new()
    {
        // 🔥 ABC Insurance Co (FIXED TENANT NAMES)
        ["ABC Insurance Co:dark-mode"] = new FeatureFlag
        {
            TenantId = "ABC Insurance Co",
            FlagKey = "dark-mode",
            Name = "Dark Mode",
            Description = "Enables dark mode for the application.",
            Enabled = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddHours(-2)
        },
        ["ABC Insurance Co:advanced-claims-search"] = new FeatureFlag
        {
            TenantId = "ABC Insurance Co",
            FlagKey = "advanced-claims-search",  // 🔥 THE FLAG YOU'RE TESTING
            Name = "Advanced Claims Search",
            Description = "Enables AI-powered advanced search capabilities.",
            Enabled = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-6)
        },
        ["ABC Insurance Co:enhanced-search"] = new FeatureFlag
        {
            TenantId = "ABC Insurance Co",
            FlagKey = "enhanced-search",
            Name = "Enhanced Search",
            Description = "Enables enhanced search capabilities.",
            Enabled = false,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddDays(-3)
        },

        // 🔥 XYZ Insurance Co
        ["XYZ Insurance Co:dark-mode"] = new FeatureFlag
        {
            TenantId = "XYZ Insurance Co",
            FlagKey = "dark-mode",
            Name = "Dark Mode",
            Description = "Enables dark mode for the application.",
            Enabled = false,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        },
        ["XYZ Insurance Co:advanced-claims-search"] = new FeatureFlag
        {
            TenantId = "XYZ Insurance Co",
            FlagKey = "advanced-claims-search",
            Name = "Advanced Claims Search",
            Description = "Enables AI-powered advanced search capabilities.",
            Enabled = true,
            CreatedAt = DateTime.UtcNow.AddDays(-4),
            UpdatedAt = DateTime.UtcNow.AddHours(-12)
        },
        ["XYZ Insurance Co:enhanced-search"] = new FeatureFlag
        {
            TenantId = "XYZ Insurance Co",
            FlagKey = "enhanced-search",
            Name = "Enhanced Search",
            Description = "Enables enhanced search capabilities.",
            Enabled = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-3)
        }
    };

    public Task<FeatureFlag?> GetFlagAsync(string tenantId, string flagKey)
    {
        var key = $"{tenantId}:{flagKey}";
        var flag = _flags.GetValueOrDefault(key);
        return Task.FromResult(flag);
    }

    public Task<List<FeatureFlag>> ListFlagsAsync(string tenantId)
    {
        var flags = _flags.Values
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.FlagKey)
            .ToList();

        return Task.FromResult(flags);
    }
}