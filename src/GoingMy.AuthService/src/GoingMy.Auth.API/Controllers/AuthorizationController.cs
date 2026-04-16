using GoingMy.Auth.API.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace GoingMy.Auth.API.Controllers;

public class AuthorizationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IRefreshTokenBlacklistService _refreshTokenBlacklistService;
    private readonly ILogger<AuthorizationController> _logger;

    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IUserService userService,
        IRefreshTokenBlacklistService refreshTokenBlacklistService,
        ILogger<AuthorizationController> logger)
    {
        _userService = userService;
        _applicationManager = applicationManager;
        _refreshTokenBlacklistService = refreshTokenBlacklistService;
        _logger = logger;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [Produces("application/json")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Check if prompt=login is requested (force re-authentication)
        if (request.Prompt == "login")
        {
            // Sign out the current user to force re-authentication
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return CreateChallengeResult();
        }

        // Check if user is authenticated via cookie session
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
        {
            // User not authenticated - redirect to login page with return URL
            return CreateChallengeResult();
        }

        // Get the user from the authenticated cookie principal
        var userId = result.Principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = Errors.InvalidRequest,
                ErrorDescription = "The user identifier cannot be retrieved."
            });
        }

        // Retrieve user details to add richer claims
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = Errors.InvalidRequest,
                ErrorDescription = "The user details cannot be retrieved."
            });
        }

        // Create claims identity and principal for authorization code
        var identity = CreateBaseClaimsIdentity();
        AddUserClaimsToIdentity(identity, user);
        var principal = new ClaimsPrincipal(identity);
        await ConfigurePrincipalForSigningAsync(principal, request.GetScopes());

        // Issue authorization code (PKCE validation happens automatically in token endpoint)
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            return await HandlePasswordGrantTypeAsync(request);
        }
        else if (request.IsClientCredentialsGrantType())
        {
            return await HandleClientCredentialsGrantTypeAsync(request);
        }
        else if (request.IsAuthorizationCodeGrantType())
        {
            return await HandleAuthorizationCodeGrantTypeAsync();
        }
        else if (request.IsRefreshTokenGrantType())
        {
            return await HandleRefreshTokenGrantTypeAsync();
        }

        return BadRequest(new OpenIddictResponse
        {
            Error = Errors.UnsupportedGrantType,
            ErrorDescription = "The specified grant type is not supported."
        });
    }

    private async Task<IActionResult> HandleRefreshTokenGrantTypeAsync()
    {
        var principal = await AuthenticateAndValidatePrincipalAsync("The refresh token is no longer valid.");
        if (principal == null)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = Errors.InvalidGrant,
                ErrorDescription = "The refresh token is no longer valid."
            });
        }

        // Check if the refresh token has been revoked (blacklisted)
        var tokenJti = principal.FindFirst("jti")?.Value;
        if (!string.IsNullOrEmpty(tokenJti))
        {
            var isRevoked = await _refreshTokenBlacklistService.IsTokenRevokedAsync(tokenJti);
            if (isRevoked)
            {
                _logger.LogWarning("Attempt to use revoked refresh token: {TokenJti}", tokenJti);
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidGrant,
                    ErrorDescription = "The refresh token is no longer valid."
                });
            }
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> HandleAuthorizationCodeGrantTypeAsync()
    {
        var principal = await AuthenticateAndValidatePrincipalAsync("The authorization code is no longer valid.");
        if (principal == null)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = Errors.InvalidGrant,
                ErrorDescription = "The authorization code is no longer valid."
            });
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> HandleClientCredentialsGrantTypeAsync(OpenIddictRequest request)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId ?? "")
                        ?? throw new InvalidOperationException("The application cannot be found.");

        var identity = CreateBaseClaimsIdentity();
        identity.AddClaim(new Claim(Claims.Subject, request.ClientId ?? "client_app"));
        identity.AddClaim(new Claim(Claims.Name, request.ClientId ?? "client_app"));

        var principal = new ClaimsPrincipal(identity);
        await ConfigurePrincipalForSigningAsync(principal, request.GetScopes());

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> HandlePasswordGrantTypeAsync(OpenIddictRequest request)
    {
        // Validate the username/password using the user service
        var user = await _userService.ValidateCredentialsAsync(request.Username ?? "", request.Password ?? "");
        if (user == null)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = Errors.InvalidGrant,
                ErrorDescription = "The username or password is invalid."
            });
        }

        // Create claims identity and principal
        var identity = CreateBaseClaimsIdentity();
        AddUserClaimsToIdentity(identity, user);
        var principal = new ClaimsPrincipal(identity);
        await ConfigurePrincipalForSigningAsync(principal, request.GetScopes());

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    public IActionResult Userinfo()
    {
        var user = HttpContext.User;

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.FindFirst(Claims.Subject)?.Value ?? string.Empty,
            [Claims.Name] = user.FindFirst(Claims.Name)?.Value ?? string.Empty
        };

        if (user.HasScope(Scopes.Email))
        {
            claims[Claims.Email] = user.FindFirst(Claims.Email)?.Value ?? string.Empty;
        }

        if (user.HasScope(Scopes.Profile))
        {
            claims[Claims.GivenName] = user.FindFirst(Claims.GivenName)?.Value ?? string.Empty;
            claims[Claims.FamilyName] = user.FindFirst(Claims.FamilyName)?.Value ?? string.Empty;
        }

        return Ok(claims);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Get the current authenticated principal to extract user info and token claims
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (result.Succeeded && result.Principal != null)
            {
                // Extract token JTI (JWT ID) claim for revocation
                var tokenJti = result.Principal.FindFirst("jti")?.Value;
                var subject = result.Principal.FindFirst(Claims.Subject)?.Value;

                // If we have both JTI and subject, revoke the refresh token
                if (!string.IsNullOrEmpty(tokenJti) && !string.IsNullOrEmpty(subject))
                {
                    // Get the configured refresh token lifetime
                    var refreshTokenLifetimeMinutes = int.Parse(
                        HttpContext.RequestServices.GetRequiredService<IConfiguration>()
                            ["OpenIddict:RefreshTokenLifetime"] ?? "60");
                    var expiresAt = DateTime.UtcNow.AddMinutes(refreshTokenLifetimeMinutes);

                    await _refreshTokenBlacklistService.RevokeTokenAsync(
                        tokenJti,
                        subject,
                        expiresAt);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token during logout");
            // Log the error but don't fail the logout process
        }

        // Clear the Identity cookie authentication
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        // Clear OpenIddict authentication if present
        await HttpContext.SignOutAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Redirect back to Angular app
        return Redirect("http://localhost:4200/");
    }

    private ClaimsIdentity CreateBaseClaimsIdentity() =>
        new(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: Claims.Name,
            roleType: Claims.Role);

    private static void AddUserClaimsToIdentity(ClaimsIdentity identity, dynamic user)
    {
        identity.AddClaim(new Claim(Claims.Subject, user.Id.ToString()));
        identity.AddClaim(new Claim(Claims.Name, user.UserName ?? string.Empty));
        identity.AddClaim(new Claim(Claims.Email, user.Email ?? string.Empty));
        identity.AddClaim(new Claim(Claims.GivenName, user.FirstName));
        identity.AddClaim(new Claim(Claims.FamilyName, user.LastName));

        foreach (var role in user.Roles)
        {
            identity.AddClaim(new Claim(Claims.Role, role.ToString()));
        }
    }

    private async Task ConfigurePrincipalForSigningAsync(ClaimsPrincipal principal, IEnumerable<string> scopes)
    {
        principal.SetScopes(scopes);
        principal.SetResources(await GetResourcesAsync(scopes));
        principal.SetDestinations(GetDestinations);
    }

    private ChallengeResult CreateChallengeResult() =>
        Challenge(
            authenticationSchemes: IdentityConstants.ApplicationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                    Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
            });

    private async Task<ClaimsPrincipal?> AuthenticateAndValidatePrincipalAsync(string errorDescription)
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        return result.Principal;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Name or Claims.Subject:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                break;

            case Claims.Email:
                yield return Destinations.IdentityToken;
                break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                break;

            default:
                yield return Destinations.AccessToken;
                break;
        }
    }

    private async Task<IEnumerable<string>> GetResourcesAsync(IEnumerable<string> scopes)
    {
        var resources = new List<string>();

        if (scopes.Contains("openid") || scopes.Contains("profile") || scopes.Contains("email"))
        {
            resources.Add("identity-server");
        }

        if (scopes.Contains("social_api"))
        {
            resources.Add("social-api");
        }

        return await Task.FromResult(resources);
    }
}
