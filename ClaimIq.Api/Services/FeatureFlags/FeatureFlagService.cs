using ClaimIq.Api.Models.FeatureFlags;

public class FeatureFlagService : IFeatureFlagService
{
    private static readonly Dictionary<string, FeatureFlag> _flags = new()
    {
        // 🔥 ABC Insurance Co (matches your JWT tenant)
        ["ABC Insurance Co:dark-mode"] = new FeatureFlag
        {
            TenantId = "ABC Insurance Co",  // 🔥 FIXED TENANT NAME
            FlagKey = "dark-mode",
            Name = "Dark Mode",
            Description = "Enables dark mode for the application.",
            Enabled = true  // 🔥 ENABLED FOR DEMO
        },
        ["ABC Insurance Co:advanced-claims-search"] = new FeatureFlag
        {
            TenantId = "ABC Insurance Co",
            FlagKey = "advanced-claims-search",  // 🔥 THE FLAG YOU'RE TESTING
            Name = "Advanced Claims Search",
            Description = "Enables AI-powered advanced search capabilities.",
            Enabled = true
        },
        ["ABC Insurance Co:enhanced-search"] = new FeatureFlag
        {
            TenantId = "ABC Insurance Co",
            FlagKey = "enhanced-search",
            Name = "Enhanced Search",
            Description = "Enables enhanced search capabilities.",
            Enabled = false
        },

        // 🔥 XYZ Insurance Co (matches your JWT tenant)
        ["XYZ Insurance Co:dark-mode"] = new FeatureFlag
        {
            TenantId = "XYZ Insurance Co",  // 🔥 FIXED TENANT NAME
            FlagKey = "dark-mode",
            Name = "Dark Mode",
            Description = "Enables dark mode for the application.",
            Enabled = false
        },
        ["XYZ Insurance Co:advanced-claims-search"] = new FeatureFlag
        {
            TenantId = "XYZ Insurance Co",
            FlagKey = "advanced-claims-search",
            Name = "Advanced Claims Search",
            Description = "Enables AI-powered advanced search capabilities.",
            Enabled = true
        },
        ["XYZ Insurance Co:enhanced-search"] = new FeatureFlag
        {
            TenantId = "XYZ Insurance Co",
            FlagKey = "enhanced-search",
            Name = "Enhanced Search",
            Description = "Enables enhanced search capabilities.",
            Enabled = true
        }
    };

    public Task<FlagEvaluationResponse> EvaluateFlagAsync(string tenantId, string flagKey)
    {
        var key = $"{tenantId}:{flagKey}";
        var flag = _flags.GetValueOrDefault(key);

        var response = new FlagEvaluationResponse
        {
            Enabled = flag?.Enabled ?? false,
            Reason = flag?.Enabled == true ? "Flag is enabled for tenant" : "Flag is disabled or not found for tenant",
            EvaluatedAt = DateTime.UtcNow
        };

        return Task.FromResult(response);
    }

    public Task<List<FeatureFlag>> ListFlagsAsync(string tenantId)
    {
        var flags = _flags.Values
            .Where(x => x.TenantId == tenantId)  // 🔥 FIXED FILTERING
            .ToList();

        return Task.FromResult(flags);
    }
}