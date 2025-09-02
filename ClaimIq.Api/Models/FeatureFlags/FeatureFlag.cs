namespace ClaimIq.Api.Models.FeatureFlags;

public class FeatureFlag
{
    public string TenantId { get; set; } = string.Empty;
    public string FlagKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
    
    // Audit fields
    public string CreatedBy { get; set; } = "system";
    public string LastModifiedBy { get; set; } = "system";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // DynamoDB keys (for future)
    public string PartitionKey => $"TENANT#{TenantId}";
    public string SortKey => $"FLAG#{FlagKey}";
}
public class FlagEvaluationRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string FlagKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, string> UserAttributes { get; set; } = new Dictionary<string, string>();
}

public class FlagEvaluationResponse
{
    public bool Enabled { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RolloutStrategy { get; set; } = string.Empty;
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    public string EvaluationId { get; set; } = string.Empty;
}