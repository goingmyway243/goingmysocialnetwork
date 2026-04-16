using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace GoingMy.Auth.API.Data;

public static class OpenIddictSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        await SeedApplicationsAsync(serviceProvider);
        await SeedScopeAsync(serviceProvider);
    }

    private static async Task SeedApplicationsAsync(IServiceProvider serviceProvider)
    {
        var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Web Client (for web application)
        if (await applicationManager.FindByClientIdAsync("web-client") is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "web-client",
                DisplayName = "Web Application Client",
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Logout,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Prefixes.Scope + "email",
                    Permissions.Prefixes.Scope + "profile",
                    Permissions.Prefixes.Scope + "roles",
                    Permissions.Prefixes.Scope + "openid",
                    Permissions.Prefixes.Scope + "social_api",
                    Permissions.Prefixes.Scope + "offline_access"
                },
                RedirectUris =
                {
                    new Uri("http://localhost:4200/signin-oidc"),
                    new Uri("http://localhost:4200/")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("http://localhost:4200/signout-callback-oidc"),
                    new Uri("http://localhost:4200/")
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            });
        }
    }

    private static async Task SeedScopeAsync(IServiceProvider serviceProvider)
    {
        var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();
        if (await scopeManager.FindByNameAsync("social_api") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "social_api",
                DisplayName = "Access to the Social API",
                Resources =
                {
                    "social-api"
                }
            });
        }
    }
}
