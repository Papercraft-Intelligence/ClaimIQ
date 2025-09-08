using Microsoft.AspNetCore.Mvc;
using ClaimIq.Api.Services;

namespace ClaimIq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtService jwtService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // 🔥 DEMO LOGIN - Validate user credentials
        // var (isValid, tenantId, userName) = ValidateUser(request.Username, request.Password);
        
        // if (!isValid)
        // {
        //     return Unauthorized(new { message = "Invalid username or password" });
        // }

       // var token = _jwtService.GenerateToken(tenantId, request.Username, userName);
        
        //_logger.LogInformation("User {Username} logged in for tenant {TenantId}", request.Username, tenantId);
        
        return Ok(new LoginResponse
        {
            Token = "123", //token,
            TenantId = "tenant1", //tenantId,
            UserName = "demo", //userName,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        });
    }

    private static (bool isValid, string tenantId, string userName) ValidateUser(string username, string password)
    {
        var demoUsers = new Dictionary<string, (string password, string tenantId, string userName)>
        {
            ["abc-admin"] = ("password123", "ABC Insurance Co", "ABC Admin"),
            ["xyz-admin"] = ("password123", "XYZ Insurance Co", "XYZ Admin"),
            ["demo"] = ("demo", "ABC Insurance Co", "Demo User")
        };

        if (demoUsers.TryGetValue(username, out var user) && user.password == password)
        {
            return (true, user.tenantId, user.userName);
        }

        return (false, "", "");
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}