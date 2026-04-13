using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System.Security.Claims;

namespace GoingMy.ServiceDefaults;

/// <summary>
/// Extension methods for adding service defaults to .NET Aspire services.
/// Includes service discovery, observability, and health checks.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Add service defaults for logging, tracing, and resilience.
    /// </summary>
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.Services.AddServiceDefaults();
        return builder;
    }

    /// <summary>
    /// Add service defaults to the service collection.
    /// </summary>
    public static IServiceCollection AddServiceDefaults(this IServiceCollection services)
    {
        // Add service discovery for inter-service communication
        services.AddServiceDiscovery();
        
        // Configure HTTP clients with service discovery
        services.ConfigureHttpClientDefaults(http =>
        {
            // Enable name resolution through service discovery
            http.AddServiceDiscovery();
            http.ConfigureHttpClient(client => 
            {
                // Service discovery is automatically applied to typed HttpClients
            });
        });

        // Add health checks support
        services.AddHealthChecks();

        // Add OpenTelemetry for distributed tracing
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                
                // Export to OTLP collector if configured
                var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    try
                    {
                        tracing.AddOtlpExporter();
                    }
                    catch
                    {
                        // OTLP exporter not available, skip
                    }
                }
            });

        return services;
    }

    /// <summary>
    /// Map service defaults such as health checks endpoint.
    /// Should be called after building the WebApplication.
    /// </summary>
    public static WebApplication MapServiceDefaults(this WebApplication app)
    {
        // Map health check endpoint for Aspire monitoring
        var healthChecks = app.MapHealthChecks("/health");
        
        // Allow anonymous access to health checks
        healthChecks.AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Use gateway authentication middleware to trust pre-authenticated requests from API Gateway.
    /// Should be called BEFORE app.UseAuthentication().
    /// </summary>
    public static WebApplication UseGatewayAuthentication(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var isGatewayAuthenticated = context.Request.Headers["X-Gateway-Authenticated"] == "true";
            
            if (isGatewayAuthenticated)
            {
                // Extract user identity from gateway-provided headers
                var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();
                var username = context.Request.Headers["X-Username"].FirstOrDefault();

                if (!string.IsNullOrEmpty(userId))
                {
                    // Build claims from gateway headers
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim("sub", userId), // Subject claim for OpenIddict compatibility
                    };

                    if (!string.IsNullOrEmpty(username))
                    {
                        claims.Add(new Claim(ClaimTypes.Name, username));
                        claims.Add(new Claim("name", username));
                    }

                    // Create an authenticated principal
                    var identity = new ClaimsIdentity(claims, "GatewayAuthentication");
                    var principal = new ClaimsPrincipal(identity);

                    // Set as the authenticated user
                    context.User = principal;
                }
            }

            await next();
        });

        return app;
    }
}


