namespace ClaimIq.Api.Models.FeatureFlags;

public class CreateFlagRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string FlagKey { get; set; } = string.Empty;
    public string Environment { get; set; } = "production";
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
    public RolloutStrategy? Rollout { get; set; }
    public Dictionary<string, object>? Variants { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdateFlagRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? Enabled { get; set; }
    public RolloutStrategy? Rollout { get; set; }
    public Dictionary<string, object>? Variants { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdateRolloutRequest
{
    public int Percentage { get; set; }
    public List<string>? UserSegments { get; set; }
    public List<string>? Countries { get; set; }
    public Dictionary<string, object>? CustomRules { get; set; }
}

public class FlagSummary
{
    public string FlagKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Environment { get; set; } = string.Empty;
    public int RolloutPercentage { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
}