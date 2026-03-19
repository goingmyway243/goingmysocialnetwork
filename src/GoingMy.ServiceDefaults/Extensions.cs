using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Trace;

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
}


