using Microsoft.AspNetCore.Mvc;
using SocialNetworkMicroservices.Identity.Models;
using SocialNetworkMicroservices.Identity.Services;
using System.Text;

namespace SocialNetworkMicroservices.Identity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginController> _logger;

    public LoginController(
        IUserService userService, 
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<LoginController> logger)
    {
        _userService = userService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Simple login endpoint that validates credentials and returns an OAuth token
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Error = "invalid_request",
                ErrorDescription = "Username and password are required."
            });
        }

        // Validate credentials
        var user = _userService.ValidateCredentials(request.Username, request.Password);
        if (user == null)
        {
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Error = "invalid_credentials",
                ErrorDescription = "The username or password is incorrect."
            });
        }

        try
        {
            // Get token from OpenIddict token endpoint
            var client = _httpClientFactory.CreateClient();
            var tokenEndpoint = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/connect/token";

            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", request.Username),
                new KeyValuePair<string, string>("password", request.Password),
                new KeyValuePair<string, string>("scope", "openid profile email")
            });

            var response = await client.PostAsync(tokenEndpoint, tokenRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                
                return Ok(new LoginResponse
                {
                    Success = true,
                    AccessToken = tokenResponse?.AccessToken,
                    TokenType = tokenResponse?.TokenType ?? "Bearer",
                    ExpiresIn = tokenResponse?.ExpiresIn,
                    RefreshToken = tokenResponse?.RefreshToken,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = user.Roles
                    }
                });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token request failed: {Error}", errorContent);
                
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Error = "token_request_failed",
                    ErrorDescription = "Failed to generate access token."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for user: {Username}", request.Username);
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Error = "internal_error",
                ErrorDescription = "An error occurred during login."
            });
        }
    }

    /// <summary>
    /// Get list of test users (for development/testing only)
    /// </summary>
    [HttpGet("test-users")]
    [ProducesResponseType(typeof(IEnumerable<TestUser>), StatusCodes.Status200OK)]
    public IActionResult GetTestUsers()
    {
        return Ok(_userService.GetAllTestUsers());
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

// DTO for token response from OpenIddict
internal class TokenResponse
{
    public string? AccessToken { get; set; }
    public string? TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
}
