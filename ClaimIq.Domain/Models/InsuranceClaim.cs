namespace ClaimIq.Domain.Models;

public class InsuranceClaim
{
    public string Description { get; set; } = string.Empty;
    public string ClaimId { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyHolderName { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty; // The company/org
    public ClaimType IncidentType { get; set; }
    public ClaimStatus Status { get; set; }
    public decimal ReserveAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal IncurredAmount => ReserveAmount + PaidAmount;
    public DateTime DateOfLoss { get; set; }
    public DateTime ReportedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string LossDescription { get; set; } = string.Empty;
    public string LossLocation { get; set; } = string.Empty;
    public ClaimSeverity Severity { get; set; }
    public string AssignedAdjuster { get; set; } = string.Empty;
    public string ClaimsExaminer { get; set; } = string.Empty;
    public RiskCategory RiskCategory { get; set; }
    public bool IsLitigated { get; set; }
    public bool IsReported { get; set; } = true;
    public string ReportingSource { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public ClaimType ClaimType { get; set; }
}

public enum ClaimType
{
    GeneralLiability,
    WorkersCompensation,
    PropertyDamage,
    AutoLiability,
    ProfessionalLiability,
    CyberSecurity,
    EmploymentPractices,
    ProductLiability,
    EnvironmentalLiability
}

public enum ClaimStatus
{
    Open,
    UnderInvestigation,
    PendingDocuments,
    InLitigation,
    Settled,
    Closed,
    Reopened,
    Denied
}

public enum ClaimSeverity
{
    Minor,      // < $10k
    Moderate,   // $10k - $100k
    Major,      // $100k - $1M
    Catastrophic // > $1M
}

public enum RiskCategory
{
    Operational,
    Financial,
    Strategic,
    Compliance,
    Reputational,
    Technology,
}