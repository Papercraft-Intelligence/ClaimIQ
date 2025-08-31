using ClaimIq.Domain.Models;

namespace ClaimIq.Api.Services;

public interface IClaimsDataService
{
    Task<ICollection<InsuranceClaim>> GetAllClaimsAsync();
    Task<InsuranceClaim?> GetClaimByIdAsync(string id);
    IEnumerable<InsuranceClaim> GetClaims();
    IEnumerable<InsuranceClaim> GetClaimsByStatus(ClaimStatus status);
    object GetClaimsSummary();
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

    private static List<InsuranceClaim> GetSampleClaims() => new()
    {
        // Your sick data here...
        new InsuranceClaim
        {
            ClaimId = "CLM-2024-WC-001",
            ClaimNumber = "WC-2024-15789",
            PolicyHolderName = "Sarah Mitchell",
            // ... rest of the data
        }
        // I'll give you the full data in next message
    };

    public Task<ICollection<InsuranceClaim>> GetAllClaimsAsync()
    {
        return Task.FromResult((ICollection<InsuranceClaim>)GetSampleClaims());
    }
}