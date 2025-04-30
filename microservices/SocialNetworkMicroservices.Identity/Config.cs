using Duende.IdentityServer.Models;

namespace SocialNetworkMicroservices.Identity;

public class Config
{
    public static IEnumerable<Client> Clients =>
        [
            new Client
            {
                ClientId = "goingmysocial_client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("goingmycLientsecRet".Sha256()) },
                AllowedScopes = { "microservice.read", "microservice.write" }
            },
            new Client
            {
                ClientId = "goingmysocial_pkce_client",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RedirectUris = { "https://localhost:5001/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },
                AllowedScopes = { "openid", "profile", "email", "microservice.read", "microservice.write" }
            }
        ];

    public static IEnumerable<ApiResource> ApiResources =>
        [
            new ApiResource("microservice.read", "Read access to the microservice"),
            new ApiResource("microservice.write", "Write access to the microservice")
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("microservice.read", "Read access to the microservice"),
            new ApiScope("microservice.write", "Write access to the microservice")
        ];

    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        ];
}
