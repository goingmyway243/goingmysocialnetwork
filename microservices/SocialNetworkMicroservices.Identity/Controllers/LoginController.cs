using Microsoft.AspNetCore.Mvc;
using SocialNetworkMicroservices.Identity.Models;
using SocialNetworkMicroservices.Identity.Services;
using System.Text;
using System.Security.Cryptography;

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
                new KeyValuePair<string, string>("scope", "admin profile email"),
                new KeyValuePair<string, string>("client_id", "swagger-client"),
                new KeyValuePair<string, string>("client_secret", "swagger-client-secret")
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
    /// PKCE login endpoint - Returns authorization code that can be exchanged for tokens
    /// This implements the Authorization Code flow with PKCE (Proof Key for Code Exchange)
    /// Note: This is a simplified API-based PKCE flow for mobile/SPA clients
    /// </summary>
    [HttpPost("pkce")]
    [ProducesResponseType(typeof(PkceLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginPkce([FromBody] PkceLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new PkceLoginResponse
            {
                Success = false,
                Error = "invalid_request",
                ErrorDescription = "Username and password are required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.CodeChallenge))
        {
            return BadRequest(new PkceLoginResponse
            {
                Success = false,
                Error = "invalid_request",
                ErrorDescription = "Code challenge is required for PKCE flow."
            });
        }

        // Validate credentials
        var user = _userService.ValidateCredentials(request.Username, request.Password);
        if (user == null)
        {
            return Unauthorized(new PkceLoginResponse
            {
                Success = false,
                Error = "invalid_credentials",
                ErrorDescription = "The username or password is incorrect."
            });
        }

        try
        {
            // Get authorization code from OpenIddict authorization endpoint using form POST
            var client = _httpClientFactory.CreateClient();
            // Configure client to not follow redirects automatically
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            var httpClient = new HttpClient(handler);

            var authEndpoint = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/connect/authorize";

            var clientId = request.ClientId ?? "mobile-client";
            var redirectUri = request.RedirectUri ?? "com.socialnetwork.app://callback";
            var scope = request.Scope ?? "profile email roles";
            var codeChallengeMethod = request.CodeChallengeMethod ?? "S256";

            // Create form content with all required parameters
            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "response_type", "code" },
                { "client_id", clientId },
                { "redirect_uri", redirectUri },
                { "scope", scope },
                { "code_challenge", request.CodeChallenge },
                { "code_challenge_method", codeChallengeMethod },
                { "username", request.Username },
                { "password", request.Password }
            });

            var response = await httpClient.PostAsync(authEndpoint, formContent);

            // Check for redirect response (302/303)
            if (response.StatusCode == System.Net.HttpStatusCode.Redirect || 
                response.StatusCode == System.Net.HttpStatusCode.SeeOther ||
                response.StatusCode == System.Net.HttpStatusCode.Found)
            {
                var location = response.Headers.Location?.ToString();
                if (!string.IsNullOrEmpty(location))
                {
                    _logger.LogInformation("Authorization redirect location: {Location}", location);

                    // Extract code from redirect URL
                    var uri = new Uri(location, UriKind.RelativeOrAbsolute);
                    if (!uri.IsAbsoluteUri && location.Contains("?"))
                    {
                        // It's a relative URI with query string
                        var queryString = location.Split('?')[1];
                        var queryParams = System.Web.HttpUtility.ParseQueryString(queryString);
                        var code = queryParams["code"];

                        if (!string.IsNullOrEmpty(code))
                        {
                            return Ok(new PkceLoginResponse
                            {
                                Success = true,
                                AuthorizationCode = code,
                                RedirectUri = redirectUri,
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
                    }
                    else if (uri.IsAbsoluteUri)
                    {
                        var query = uri.Query;
                        if (!string.IsNullOrEmpty(query))
                        {
                            var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                            var code = queryParams["code"];

                            if (!string.IsNullOrEmpty(code))
                            {
                                return Ok(new PkceLoginResponse
                                {
                                    Success = true,
                                    AuthorizationCode = code,
                                    RedirectUri = redirectUri,
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
                        }
                    }
                }
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Authorization request failed with status {StatusCode}. Response: {Response}", 
                response.StatusCode, responseContent);

            return Unauthorized(new PkceLoginResponse
            {
                Success = false,
                Error = "authorization_request_failed",
                ErrorDescription = $"Failed to generate authorization code. Status: {response.StatusCode}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PKCE login error for user: {Username}", request.Username);
            return StatusCode(500, new PkceLoginResponse
            {
                Success = false,
                Error = "internal_error",
                ErrorDescription = "An error occurred during PKCE login."
            });
        }
    }

    /// <summary>
    /// Exchange authorization code for access token using PKCE
    /// </summary>
    [HttpPost("pkce/token")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExchangePkceCode([FromBody] PkceTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.CodeVerifier))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Error = "invalid_request",
                ErrorDescription = "Authorization code and code verifier are required."
            });
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var tokenEndpoint = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/connect/token";

            var clientId = request.ClientId ?? "mobile-client";
            var redirectUri = request.RedirectUri ?? "com.socialnetwork.app://callback";

            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", request.Code),
                new KeyValuePair<string, string>("code_verifier", request.CodeVerifier),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
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
                    RefreshToken = tokenResponse?.RefreshToken
                });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token exchange failed: {Error}", errorContent);

                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Error = "token_exchange_failed",
                    ErrorDescription = "Failed to exchange authorization code for tokens."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token exchange error");
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Error = "internal_error",
                ErrorDescription = "An error occurred during token exchange."
            });
        }
    }

    /// <summary>
    /// Helper endpoint to generate PKCE parameters for testing
    /// </summary>
    [HttpGet("pkce/generate")]
    [ProducesResponseType(typeof(PkceParameters), StatusCodes.Status200OK)]
    public IActionResult GeneratePkceParameters()
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        return Ok(new PkceParameters
        {
            CodeVerifier = codeVerifier,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = "S256"
        });
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

    // Helper methods for PKCE
    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

// DTO for token response from OpenIddict
internal class TokenResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}
