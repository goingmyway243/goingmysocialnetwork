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
    private readonly IOpenIddictAuthorizationManager _authorizationManager;

    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IUserService userService,
        IOpenIddictAuthorizationManager authorizationManager)
    {
        _userService = userService;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
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
            
            // Redirect to login page
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // Check if user is authenticated via cookie session
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
        {
            // User not authenticated - redirect to login page with return URL
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
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

        // Create claims identity for authorization code
        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.AddClaim(new Claim(Claims.Subject, user.Id.ToString()));
        identity.AddClaim(new Claim(Claims.Name, user.UserName ?? string.Empty));
        identity.AddClaim(new Claim(Claims.Email, user.Email ?? string.Empty));
        identity.AddClaim(new Claim(Claims.GivenName, user.FirstName));
        identity.AddClaim(new Claim(Claims.FamilyName, user.LastName));

        // Add roles
        foreach (var role in user.Roles)
        {
            identity.AddClaim(new Claim(Claims.Role, role.ToString()));
        }

        var principal = new ClaimsPrincipal(identity);

        // Set requested scopes and resources
        principal.SetScopes(request.GetScopes());
        principal.SetResources(await GetResourcesAsync(request.GetScopes()));
        principal.SetDestinations(GetDestinations);

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

            // Create claims identity
            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.AddClaim(new Claim(Claims.Subject, user.Id.ToString()));
            identity.AddClaim(new Claim(Claims.Name, user.UserName ?? string.Empty));
            identity.AddClaim(new Claim(Claims.Email, user.Email ?? string.Empty));
            identity.AddClaim(new Claim("given_name", user.FirstName));
            identity.AddClaim(new Claim("family_name", user.LastName));

            // Add roles
            foreach (var role in user.Roles)
            {
                identity.AddClaim(new Claim(Claims.Role, role.ToString()));
            }

            // Set scopes
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else if (request.IsClientCredentialsGrantType())
        {
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId ?? "")
                ?? throw new InvalidOperationException("The application cannot be found.");

            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.AddClaim(new Claim(Claims.Subject, request.ClientId ?? "client_app"));
            identity.AddClaim(new Claim(Claims.Name, request.ClientId ?? "client_app"));

            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else if (request.IsAuthorizationCodeGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (result.Principal == null)
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidGrant,
                    ErrorDescription = "The authorization code is no longer valid."
                });
            }

            return SignIn(result.Principal,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else if (request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (result.Principal == null)
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = Errors.InvalidGrant,
                    ErrorDescription = "The refresh token is no longer valid."
                });
            }

            return SignIn(result.Principal,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest(new OpenIddictResponse
        {
            Error = Errors.UnsupportedGrantType,
            ErrorDescription = "The specified grant type is not supported."
        });
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

        return Ok(claims);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        // Clear the Identity cookie authentication
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        
        // Clear OpenIddict authentication if present
        await HttpContext.SignOutAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Redirect back to Angular app
        return Redirect("http://localhost:4200/");
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
