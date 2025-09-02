using ClaimIq.Api.Models.FeatureFlags;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ClaimIq.Api.Services.FeatureFlags;

public class RedisFeatureFlagRepository : IFeatureFlagRepository
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisFeatureFlagRepository> _logger;
    private const string FLAG_PREFIX = "feature_flag:";
    private const string TENANT_FLAGS_PREFIX = "tenant_flags:";

    public RedisFeatureFlagRepository(IDistributedCache cache, ILogger<RedisFeatureFlagRepository> logger)
    {
        _cache = cache;
        _logger = logger;
        
        // 🔥 INITIALIZE DEMO FLAGS ON STARTUP
        _ = Task.Run(InitializeDemoFlags);
    }

    public async Task<FeatureFlag?> GetFlagAsync(string tenantId, string flagKey)
    {
        try
        {
            var cacheKey = $"{FLAG_PREFIX}{tenantId}:{flagKey}";
            _logger.LogInformation("🔥 REDIS: Getting feature flag {TenantId}:{FlagKey}", tenantId, flagKey);
            
            // 🔥 ADD TIMEOUT FOR INDIVIDUAL OPERATIONS
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var json = await _cache.GetStringAsync(cacheKey, cts.Token);
            
            if (string.IsNullOrEmpty(json))
            {
                _logger.LogWarning("❌ REDIS: Feature flag {TenantId}:{FlagKey} not found", tenantId, flagKey);
                return null;
            }

            var flag = JsonSerializer.Deserialize<FeatureFlag>(json);
            _logger.LogInformation("✅ REDIS: Retrieved feature flag {TenantId}:{FlagKey} = {Enabled}", tenantId, flagKey, flag?.Enabled);
            return flag;
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("⏰ REDIS TIMEOUT: Feature flag {TenantId}:{FlagKey} operation timed out", tenantId, flagKey);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 REDIS ERROR: Failed to get feature flag {TenantId}:{FlagKey}", tenantId, flagKey);
            return null;
        }
    }

    public async Task<List<FeatureFlag>> ListFlagsAsync(string tenantId)
    {
        try
        {
            _logger.LogInformation("🔥 REDIS: Getting all feature flags for tenant {TenantId}", tenantId);
            
            // Get list of flag keys for this tenant
            var tenantFlagsKey = $"{TENANT_FLAGS_PREFIX}{tenantId}";
            var flagKeysJson = await _cache.GetStringAsync(tenantFlagsKey);
            
            if (string.IsNullOrEmpty(flagKeysJson))
            {
                _logger.LogWarning("❌ REDIS: No flags found for tenant {TenantId}", tenantId);
                return new List<FeatureFlag>();
            }

            var flagKeys = JsonSerializer.Deserialize<List<string>>(flagKeysJson) ?? new List<string>();
            var flags = new List<FeatureFlag>();

            // Get each flag
            foreach (var flagKey in flagKeys)
            {
                var flag = await GetFlagAsync(tenantId, flagKey);
                if (flag != null)
                {
                    flags.Add(flag);
                }
            }

            flags = flags.OrderBy(f => f.FlagKey).ToList();
            _logger.LogInformation("✅ REDIS: Retrieved {Count} feature flags for tenant {TenantId}", flags.Count, tenantId);
            return flags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 REDIS ERROR: Failed to list feature flags for tenant {TenantId}", tenantId);
            return new List<FeatureFlag>();
        }
    }

    // 🔥 HELPER METHODS FOR REDIS MANAGEMENT
    public async Task SetFlagAsync(string tenantId, string flagKey, FeatureFlag flag)
    {
        try
        {
            var cacheKey = $"{FLAG_PREFIX}{tenantId}:{flagKey}";
            var json = JsonSerializer.Serialize(flag);
            
            _logger.LogInformation("🔥 REDIS: Setting feature flag {TenantId}:{FlagKey} = {Enabled}", tenantId, flagKey, flag.Enabled);
            
            // Set flag with 1 hour expiration
            await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

            // Update tenant's flag list
            await AddFlagToTenantListAsync(tenantId, flagKey);
            
            _logger.LogInformation("✅ REDIS: Successfully set feature flag {TenantId}:{FlagKey}", tenantId, flagKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 REDIS ERROR: Failed to set feature flag {TenantId}:{FlagKey}", tenantId, flagKey);
        }
    }

    public async Task DeleteFlagAsync(string tenantId, string flagKey)
    {
        try
        {
            var cacheKey = $"{FLAG_PREFIX}{tenantId}:{flagKey}";
            _logger.LogInformation("🔥 REDIS: Deleting feature flag {TenantId}:{FlagKey}", tenantId, flagKey);
            
            await _cache.RemoveAsync(cacheKey);
            await RemoveFlagFromTenantListAsync(tenantId, flagKey);
            
            _logger.LogInformation("✅ REDIS: Successfully deleted feature flag {TenantId}:{FlagKey}", tenantId, flagKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 REDIS ERROR: Failed to delete feature flag {TenantId}:{FlagKey}", tenantId, flagKey);
        }
    }

    // 🔥 TENANT FLAG LIST MANAGEMENT
    private async Task AddFlagToTenantListAsync(string tenantId, string flagKey)
    {
        try
        {
            var tenantFlagsKey = $"{TENANT_FLAGS_PREFIX}{tenantId}";
            var existingJson = await _cache.GetStringAsync(tenantFlagsKey);
            
            var flagKeys = string.IsNullOrEmpty(existingJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(existingJson) ?? new List<string>();

            if (!flagKeys.Contains(flagKey))
            {
                flagKeys.Add(flagKey);
                var json = JsonSerializer.Serialize(flagKeys);
                await _cache.SetStringAsync(tenantFlagsKey, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant flag list for {TenantId}", tenantId);
        }
    }

    private async Task RemoveFlagFromTenantListAsync(string tenantId, string flagKey)
    {
        try
        {
            var tenantFlagsKey = $"{TENANT_FLAGS_PREFIX}{tenantId}";
            var existingJson = await _cache.GetStringAsync(tenantFlagsKey);
            
            if (!string.IsNullOrEmpty(existingJson))
            {
                var flagKeys = JsonSerializer.Deserialize<List<string>>(existingJson) ?? new List<string>();
                flagKeys.Remove(flagKey);
                
                var json = JsonSerializer.Serialize(flagKeys);
                await _cache.SetStringAsync(tenantFlagsKey, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant flag list for {TenantId}", tenantId);
        }
    }

    private async Task InitializeDemoFlags()
    {
        await Task.Delay(3000); // Wait for Redis to be ready
        
        _logger.LogInformation("🔥 REDIS: Initializing demo feature flags...");

        var demoFlags = new[]
        {
            // ABC Insurance Co flags
            new FeatureFlag 
            { 
                TenantId = "ABC Insurance Co",
                FlagKey = "dark-mode", 
                Name = "Dark Mode", 
                Description = "Enables dark mode for the application.",
                Enabled = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new FeatureFlag 
            { 
                TenantId = "ABC Insurance Co",
                FlagKey = "advanced-claims-search",  // 🔥 ADD THIS FLAG
                Name = "Advanced Claims Search", 
                Description = "Enables AI-powered advanced search capabilities.",
                Enabled = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new FeatureFlag 
            { 
                TenantId = "ABC Insurance Co",
                FlagKey = "enhanced-search", 
                Name = "Enhanced Search", 
                Description = "Enables enhanced search capabilities.",
                Enabled = false,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            
            // XYZ Insurance Co flags  
            new FeatureFlag 
            { 
                TenantId = "XYZ Insurance Co",
                FlagKey = "dark-mode", 
                Name = "Dark Mode", 
                Description = "Enables dark mode for the application.",
                Enabled = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new FeatureFlag 
            { 
                TenantId = "XYZ Insurance Co",
                FlagKey = "advanced-claims-search",  // 🔥 ADD THIS FLAG
                Name = "Advanced Claims Search", 
                Description = "Enables AI-powered advanced search capabilities.",
                Enabled = true,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new FeatureFlag 
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

        foreach (var flag in demoFlags)
        {
            await SetFlagAsync(flag.TenantId, flag.FlagKey, flag);
        }
        
        _logger.LogInformation("✅ REDIS: Successfully initialized {Count} demo feature flags", demoFlags.Length);
    }
}