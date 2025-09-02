using System.Text.Json;
using ClaimIq.Api.Models.FeatureFlags;
using Microsoft.Extensions.Caching.Distributed;

namespace ClaimIq.Api.Services.FeatureFlags;

public class CachedFeatureFlagService : IFeatureFlagService  // 🔥 IMPLEMENT INTERFACE
{
    private readonly IDistributedCache _cache;
    private readonly IFeatureFlagRepository _featureFlagRepository;
    private readonly ILogger<CachedFeatureFlagService> _logger;

    public CachedFeatureFlagService(IDistributedCache cache, IFeatureFlagRepository featureFlagRepository, ILogger<CachedFeatureFlagService> logger)
    {
        _cache = cache;
        _featureFlagRepository = featureFlagRepository;
        _logger = logger;
    }

    public async Task<FlagEvaluationResponse> EvaluateFlagAsync(string tenantId, string flagKey)
    {
        var cacheKey = $"flag:{tenantId}:{flagKey}";

        FeatureFlag? featureFlag = null;

        var cachedFlagBytes = await _cache.GetAsync(cacheKey);

        // Get from cache
        if (cachedFlagBytes != null)
        {
            var cachedJson = System.Text.Encoding.UTF8.GetString(cachedFlagBytes);
            featureFlag = JsonSerializer.Deserialize<FeatureFlag>(cachedJson);
            _logger.LogDebug("Retrieved feature flag from cache: {FeatureFlag}", featureFlag?.FlagKey);
        }

        // Cache miss - get from repository
        if (featureFlag == null)
        {
            _logger.LogInformation("Cache miss for key {CacheKey}. Fetching from repository.", cacheKey);
            var flag = await _featureFlagRepository.GetFlagAsync(tenantId, flagKey);
            
            if (flag != null)
            {
                featureFlag = flag;  // 🔥 FIXED: flag is already FeatureFlag, not byte[]

                // Cache the flag as JSON bytes
                var flagJson = JsonSerializer.Serialize(featureFlag);
                var flagBytes = System.Text.Encoding.UTF8.GetBytes(flagJson);
                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                await _cache.SetAsync(cacheKey, flagBytes, options);  // 🔥 FIXED: Store bytes, not FeatureFlag
                _logger.LogInformation("Cached feature flag with key {CacheKey}", cacheKey);
            }
            else
            {
                _logger.LogWarning("Feature flag {FlagKey} not found for tenant {TenantId}", flagKey, tenantId);
                return new FlagEvaluationResponse
                {
                    Enabled = false,
                    Reason = "Flag not found",
                    EvaluationId = Guid.NewGuid().ToString(),
                    EvaluatedAt = DateTime.UtcNow
                };
            }
        }

        return featureFlag != null
            ? new FlagEvaluationResponse
            {
                Enabled = featureFlag.Enabled,
                Reason = featureFlag.Enabled ? "Flag is enabled" : "Flag is disabled",
                RolloutStrategy = featureFlag.Enabled ? "Standard rollout" : "N/A",
                EvaluationId = Guid.NewGuid().ToString(),
                EvaluatedAt = DateTime.UtcNow
            }
            : new FlagEvaluationResponse
            {
                Enabled = false,
                Reason = "Flag not found after retrieval",
                EvaluationId = Guid.NewGuid().ToString(),
                EvaluatedAt = DateTime.UtcNow
            };
    }

    // 🔥 IMPLEMENT OTHER INTERFACE METHODS
    public async Task<List<FeatureFlag>> ListFlagsAsync(string tenantId)
    {
        return await _featureFlagRepository.ListFlagsAsync(tenantId);
    }

    public async Task<FeatureFlag?> GetFlagAsync(string tenantId, string flagKey)
    {
        return await _featureFlagRepository.GetFlagAsync(tenantId, flagKey);
    }
}

public interface IFeatureFlagRepository
{
    Task<FeatureFlag?> GetFlagAsync(string tenantId, string flagKey);  // 🔥 FIXED: Returns FeatureFlag, not byte[]
    Task<List<FeatureFlag>> ListFlagsAsync(string tenantId);
}