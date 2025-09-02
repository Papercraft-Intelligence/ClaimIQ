using Microsoft.AspNetCore.Mvc;
using ClaimIq.Domain.Models;
using ClaimIq.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace ClaimIq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClaimsController : ControllerBase
{
    private readonly IClaimsDataService _claimsDataService;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(IClaimsDataService claimsDataService, ILogger<ClaimsController> logger)
    {
        _claimsDataService = claimsDataService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InsuranceClaim>>> GetClaims()  // 🔥 NO PARAMETERS!
    {
        // 🔥 EXTRACT TENANT FROM JWT TOKEN
        var tenantId = User.FindFirst("tenant_id")?.Value;
        var userName = User.FindFirst("name")?.Value;
        
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant_id found in JWT token");
            return Forbid("No tenant access");
        }

        _logger.LogInformation("Getting claims for user {UserName} in tenant {TenantId}", userName, tenantId);
        
        try
        {
            var claims = await _claimsDataService.GetAllClaimsAsync(tenantId);
            _logger.LogInformation("Returned {Count} claims for tenant {TenantId}", claims.Count, tenantId);
            return Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting claims for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InsuranceClaim>> GetClaim(string id)
    {
        var tenantId = User.FindFirst("tenant_id")?.Value;
        
        if (string.IsNullOrEmpty(tenantId))
        {
            return Forbid("No tenant access");
        }

        try
        {
            var claim = await _claimsDataService.GetClaimByIdAsync(id);
            
            if (claim == null || claim.EntityName != tenantId)
            {
                return NotFound();
            }
            
            return Ok(claim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting claim {ClaimId} for tenant {TenantId}", id, tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("me")]
    public IActionResult GetUserInfo()
    {
        return Ok(new
        {
            TenantId = User.FindFirst("tenant_id")?.Value,
            UserName = User.FindFirst("name")?.Value,
            UserId = User.FindFirst("sub")?.Value,
            Role = User.FindFirst("role")?.Value,
            AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }
}