using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SocialNetworkMicroservices.Identity.Services;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SocialNetworkMicroservices.Identity;

public class AuthorizationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public AuthorizationController(IOpenIddictApplicationManager applicationManager, IUserService userService)
    {
        _userService = userService;
        _applicationManager = applicationManager;
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
            var user = _userService.ValidateCredentials(request.Username ?? "", request.Password ?? "");
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

            identity.AddClaim(Claims.Subject, user.Id);
            identity.AddClaim(Claims.Name, user.Username);
            identity.AddClaim(Claims.Email, user.Email);
            identity.AddClaim("given_name", user.FirstName);
            identity.AddClaim("family_name", user.LastName);
            
            // Add roles
            foreach (var role in user.Roles)
            {
                identity.AddClaim(Claims.Role, role);
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

            identity.AddClaim(Claims.Subject, request.ClientId ?? "client_app");
            identity.AddClaim(Claims.Name, request.ClientId ?? "client_app");

            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), 
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
}
