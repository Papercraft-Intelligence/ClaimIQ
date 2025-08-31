using Microsoft.AspNetCore.Mvc;
using ClaimIq.Domain.Models;
using ClaimIq.Api.Services;

namespace ClaimIq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimsDataService _claimsDataService;

    public ClaimsController(IClaimsDataService claimsDataService)
    {
        _claimsDataService = claimsDataService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InsuranceClaim>>> GetClaims()
    {
        var claims = await _claimsDataService.GetAllClaimsAsync();
        return Ok(claims);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InsuranceClaim>> GetClaim(string id)
    {
        var claim = await _claimsDataService.GetClaimByIdAsync(id);
        if (claim == null)
            return NotFound();
        return Ok(claim);
    }

}