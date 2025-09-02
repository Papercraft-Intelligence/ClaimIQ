namespace ClaimIq.Api.Models.FeatureFlags;

public class EvaluationRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string Environment { get; set; } = "production";
    public string UserId { get; set; } = string.Empty;
    public List<string> FlagKeys { get; set; } = new();
    public Dictionary<string, object> UserContext { get; set; } = new();
}

public class EvaluationResult
{
    public bool Enabled { get; set; }                           // tells the client if the feature flag is enabled
    public object? Variant { get; set; }                        // a variant is the specific variation (if any, ex: "beta") of the feature flag
    public string Reason { get; set; } = String.Empty;           // the reason for the evaluation result, such as "user is in beta group"
    public string Strategy { get; set; } = String.Empty;       // the strategy used to evaluate the feature flag
    public DateTime EvaluatedAt { get; set; }                   // the timestamp when the evaluation was performed
}

public class EvaluationResponse
{
    public Dictionary<string, EvaluationResult> Flags { get; set; } = new();
    public string EvaluationId { get; set; } = Guid.NewGuid().ToString();
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; } = TimeSpan.Zero;
}

partial class BatchEvaluationRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string Environment { get; set; } = "production";
    public List<UserEvaluationRequest> Requests { get; set; } = new();
}

public class UserEvaluationRequest
{
    public string UserId { get; private set; } = string.Empty;
    public List<string> FlagKeys { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
}