using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SocialNetworkMicroservices.Identity.Data;

public static class OpenIddictSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        await SeedApplicationsAsync(serviceProvider);
        await SeedScopesAsync(serviceProvider);
    }

    private static async Task SeedApplicationsAsync(IServiceProvider serviceProvider)
    {
        var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Service Worker Client (for microservices communication)
        if (await applicationManager.FindByClientIdAsync("service-worker") is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "service-worker",
                ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207",
                DisplayName = "Service Worker Client",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.Prefixes.Scope + "admin"
                }
            });
        }

        // Web Client (for web application)
        if (await applicationManager.FindByClientIdAsync("web-client") is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "web-client",
                ClientSecret = "web-client-secret",
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
                    Permissions.Prefixes.Scope + "roles"
                },
                RedirectUris =
                {
                    new Uri("https://localhost:7001/signin-oidc")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:7001/signout-callback-oidc")
                }
            });
        }

        // Mobile Client (for mobile applications)
        if (await applicationManager.FindByClientIdAsync("mobile-client") is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "mobile-client",
                DisplayName = "Mobile Application Client",
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Prefixes.Scope + "email",
                    Permissions.Prefixes.Scope + "profile",
                    Permissions.Prefixes.Scope + "roles"
                },
                RedirectUris =
                {
                    new Uri("com.socialnetwork.app://callback")
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            });
        }

        // Swagger Client (for API testing)
        if (await applicationManager.FindByClientIdAsync("swagger-client") is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "swagger-client",
                ClientSecret = "swagger-client-secret",
                DisplayName = "Swagger UI Client",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.Password,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Prefixes.Scope + "email",
                    Permissions.Prefixes.Scope + "profile",
                    Permissions.Prefixes.Scope + "roles",
                    Permissions.Prefixes.Scope + "admin"
                }
            });
        }
    }

    private static async Task SeedScopesAsync(IServiceProvider serviceProvider)
    {
        var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();

        // Email scope
        if (await scopeManager.FindByNameAsync("email") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "email",
                DisplayName = "Email",
                Description = "Access to user email address",
                Resources =
                {
                    "identity-server"
                }
            });
        }

        // Profile scope
        if (await scopeManager.FindByNameAsync("profile") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "profile",
                DisplayName = "Profile",
                Description = "Access to user profile information",
                Resources =
                {
                    "identity-server"
                }
            });
        }

        // Roles scope
        if (await scopeManager.FindByNameAsync("roles") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "roles",
                DisplayName = "Roles",
                Description = "Access to user roles",
                Resources =
                {
                    "identity-server"
                }
            });
        }

        // Admin scope
        if (await scopeManager.FindByNameAsync("admin") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "admin",
                DisplayName = "Admin",
                Description = "Access to admin features",
                Resources =
                {
                    "identity-server"
                }
            });
        }
    }
}
