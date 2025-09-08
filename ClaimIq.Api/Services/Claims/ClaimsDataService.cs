using ClaimIq.Domain.Models;

namespace ClaimIq.Api.Services;

public interface IClaimsDataService
{
    Task<ICollection<InsuranceClaim>> GetAllClaimsAsync(string tenantId);  // 🔥 FIXED: tenantId not tenandId
    Task<InsuranceClaim?> GetClaimByIdAsync(string id);
    IEnumerable<InsuranceClaim> GetClaims();
    IEnumerable<InsuranceClaim> GetClaimsByStatus(ClaimStatus status);
    object GetClaimsSummary();
    List<InsuranceClaim> GetSampleClaims();
}

public class ClaimsDataService : IClaimsDataService
{
    public IEnumerable<InsuranceClaim> GetClaims()
    {
        return GetSampleClaims().OrderByDescending(c => c.ReportedDate);
    }

    public async Task<InsuranceClaim?> GetClaimByIdAsync(string id)
    {
        return await Task.FromResult(GetSampleClaims().FirstOrDefault(c => c.ClaimId == id));
    }

    public IEnumerable<InsuranceClaim> GetClaimsByStatus(ClaimStatus status)
    {
        return GetSampleClaims().Where(c => c.Status == status).OrderByDescending(c => c.ReportedDate);
    }

    public object GetClaimsSummary()
    {
        var claims = GetSampleClaims();
        return new
        {
            TotalClaims = claims.Count(),
            TotalReserveAmount = claims.Sum(c => c.ReserveAmount),
            TotalPaidAmount = claims.Sum(c => c.PaidAmount),
            OpenClaims = claims.Count(c => c.Status == ClaimStatus.Open || c.Status == ClaimStatus.UnderInvestigation),
            InLitigation = claims.Count(c => c.IsLitigated)
        };
    }

    // 🔥 FIXED: Multi-tenant claims with proper tenant isolation
    public Task<ICollection<InsuranceClaim>> GetAllClaimsAsync(string tenantId)
    {
        var allClaims = GetSampleClaims()
            .Where(c => c.EntityName == tenantId)  // 🔥 USE EntityName as tenant identifier
            .ToList();
        
        return Task.FromResult((ICollection<InsuranceClaim>)allClaims);  // 🔥 PROPER Task.FromResult
    }

    // 🔥 ENHANCED: Multi-tenant sample data
    public List<InsuranceClaim> GetSampleClaims() => new()
    {
        new InsuranceClaim
        {
            ClaimId = "CLM-2024-ABC-001",
            ClaimNumber = "WC-2024-15789",
            PolicyHolderName = "Sarah Mitchell",
            EntityName = "ABC Insurance Co",  // 🔥 TENANT 1
            ClaimType = ClaimType.WorkersCompensation,
            Status = ClaimStatus.Open,
            ReportedDate = DateTime.Now.AddDays(-15),
            DateOfLoss  = DateTime.Now.AddDays(-20),
            ReserveAmount = 75000.00m,
            PaidAmount = 12500.00m,
            IsLitigated = false,
            Description = "Employee injury while operating machinery",
            AssignedAdjuster = "Mike Johnson",
            LossLocation = "Manufacturing Plant - Building A"
        },
        new InsuranceClaim
        {
            ClaimId = "CLM-2024-ABC-002",
            ClaimNumber = "GL-2024-28934",
            PolicyHolderName = "Downtown Restaurant LLC",
            EntityName = "ABC Insurance Co",  // 🔥 SAME TENANT
            ClaimType = ClaimType.GeneralLiability,
            Status = ClaimStatus.UnderInvestigation,
            ReportedDate = DateTime.Now.AddDays(-8),
            DateOfLoss = DateTime.Now.AddDays(-10),
            ReserveAmount = 125000.00m,
            PaidAmount = 5000.00m,
            IsLitigated = true,
            Description = "Customer slip and fall incident",
            AssignedAdjuster = "Lisa Chen",
            LossLocation = "Main Dining Area"
        },
        new InsuranceClaim
        {
            ClaimId = "CLM-2024-XYZ-001",
            ClaimNumber = "AUTO-2024-44521",
            PolicyHolderName = "John Smith",
            EntityName = "XYZ Insurance Co",  // 🔥 TENANT 2
            ClaimType = ClaimType.AutoLiability,
            Status = ClaimStatus.Closed,
            ReportedDate = DateTime.Now.AddDays(-45),
            DateOfLoss = DateTime.Now.AddDays(-50),
            ReserveAmount = 25000.00m,
            PaidAmount = 18750.00m,
            IsLitigated = false,
            Description = "Rear-end collision at intersection",
            AssignedAdjuster = "David Rodriguez",
            LossLocation = "Main St & Oak Ave"
        },
        new InsuranceClaim
        {
            ClaimId = "CLM-2024-XYZ-002",
            ClaimNumber = "PROP-2024-67890",
            PolicyHolderName = "Tech Startup Inc",
            EntityName = "XYZ Insurance Co",  // 🔥 SAME TENANT 2
            ClaimType = ClaimType.PropertyDamage,
            Status = ClaimStatus.Open,
            ReportedDate = DateTime.Now.AddDays(-3),
            DateOfLoss = DateTime.Now.AddDays(-5),
            ReserveAmount = 200000.00m,
            PaidAmount = 0.00m,
            IsLitigated = false,
            Description = "Water damage from burst pipe",
            AssignedAdjuster = "Amanda Thompson",
            LossLocation = "Office Building - 3rd Floor"
        }
    };
}