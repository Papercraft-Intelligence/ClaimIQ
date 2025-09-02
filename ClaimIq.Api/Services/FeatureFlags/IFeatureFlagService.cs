using ClaimIq.Api.Models.FeatureFlags;

public interface IFeatureFlagService
{
    Task<FlagEvaluationResponse> EvaluateFlagAsync(string tenantId, string flagKey);
    Task<List<FeatureFlag>> ListFlagsAsync(string tenantId);
}
