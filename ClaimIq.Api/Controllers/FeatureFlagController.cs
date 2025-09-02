using Microsoft.AspNetCore.Mvc;
using ClaimIq.Api.Services.FeatureFlags;
using Microsoft.AspNetCore.Authorization;

namespace ClaimIq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // 🔥 REQUIRE JWT FOR FEATURE FLAGS TOO
public class FeatureFlagController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ILogger<FeatureFlagController> _logger;

    public FeatureFlagController(IFeatureFlagService featureFlagService, ILogger<FeatureFlagController> logger)
    {
        _featureFlagService = featureFlagService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFlags()
    {
        // 🔥 GET TENANT FROM JWT TOKEN
        var tenantId = User.FindFirst("tenant_id")?.Value;
        
        if (string.IsNullOrEmpty(tenantId))
        {
            return Forbid("No tenant access");
        }

        _logger.LogInformation("Getting feature flags for tenant {TenantId}", tenantId);
        
        try
        {
            var flags = await _featureFlagService.ListFlagsAsync(tenantId);
            return Ok(flags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature flags for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("evaluate")]
    public async Task<IActionResult> EvaluateFlag(string flagKey)
    {
        // 🔥 GET TENANT FROM JWT TOKEN
        var tenantId = User.FindFirst("tenant_id")?.Value;
        var userName = User.FindFirst("name")?.Value;
        
        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(flagKey))
        {
            return BadRequest(new { message = "TenantId and flagKey are required" });
        }

        _logger.LogInformation("Evaluating flag {FlagKey} for user {UserName} in tenant {TenantId}", flagKey, userName, tenantId);
        
        try
        {
            var result = await _featureFlagService.EvaluateFlagAsync(tenantId, flagKey);
            return Ok(new 
            { 
                flagKey,
                tenantId,
                userName,
                enabled = result.Enabled,
                reason = result.Reason
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating feature flag {FlagKey} for tenant {TenantId}", flagKey, tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // 🔥 DEMO ENDPOINT - Show all flags for current user's tenant
    [HttpGet("my-flags")]
    public async Task<IActionResult> GetMyFlags()
    {
        var tenantId = User.FindFirst("tenant_id")?.Value;
        var userName = User.FindFirst("name")?.Value;
        
        if (string.IsNullOrEmpty(tenantId))
        {
            return Forbid("No tenant access");
        }

        try
        {
            var flags = await _featureFlagService.ListFlagsAsync(tenantId);
            return Ok(new
            {
                tenantId,
                userName,
                flags,
                totalFlags = flags.Count,
                enabledFlags = flags.Count(f => f.Enabled)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flags for user {UserName} in tenant {TenantId}", userName, tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}