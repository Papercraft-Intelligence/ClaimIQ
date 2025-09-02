namespace ClaimIq.Domain.Models;

public class FeatureFlag
{
    public string FlagKey { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Enabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FeatureFlagEvaluationResult
{
    public string FlagKey { get; set; } = "";
    public string TenantId { get; set; } = "";
    public string UserName { get; set; } = "";
    public bool Enabled { get; set; }
    public string Reason { get; set; } = "";
}