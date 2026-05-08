using GoingMy.ApiGateway.Middleware;
using GoingMy.ServiceDefaults;
using GoingMy.Shared;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Validation.AspNetCore;
using StackExchange.Redis;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Transforms;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// ── Reverse Proxy ─────────────────────────────────────────────
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver()
    .AddTransforms(transformBuilderContext =>
    {
        // Forward authenticated user claims as request headers to downstream services
        transformBuilderContext.AddRequestTransform(async transformContext =>
        {
            // Strip client-supplied auth headers to prevent spoofing
            transformContext.ProxyRequest.Headers.Remove("X-Gateway-Authenticated");
            transformContext.ProxyRequest.Headers.Remove("X-User-Id");
            transformContext.ProxyRequest.Headers.Remove("X-Username");

            var user = transformContext.HttpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? user.FindFirstValue("sub");
                var username = user.FindFirstValue(ClaimTypes.Name)
                               ?? user.FindFirstValue("name");

                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Gateway-Authenticated", "true");
                if (userId is not null)
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);
                if (username is not null)
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Username", username);
            }
            await Task.CompletedTask;
        });
    });

// ── OpenIddict JWT validation ─────────────────────────────────
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(builder.Configuration["OpenIddict:Issuer"]!);
        options.UseSystemNetHttp();
        options.UseAspNetCore();
        options.AddAudiences("social-api");
    });

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization(options =>
{
    // admin-policy: used by /api/admin/** and /api/posts/admin/** gateway routes
    options.AddPolicy("admin-policy", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim(Claims.Role, "Admin"));
});

// ── Redis (for token revocation middleware) ───────────────────
var redisConnectionString = builder.Configuration.GetConnectionString(SharedServices.Redis) ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration["CorsOrigins"]!.Split(',');
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // required for SignalR WebSocket upgrade
    });
});

// ── Rate Limiting ─────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api-rate-limit", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromSeconds(10);
        limiterOptions.PermitLimit = 100;
        limiterOptions.QueueLimit = 0;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// Allow large uploads to be proxied (440 MB = 4 × 100 MB video + overhead)
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/uploads"))
        context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>()!
            .MaxRequestBodySize = 440 * 1024 * 1024;
    await next(context);
});

app.UseWebSockets(); // must precede YARP for SignalR WebSocket proxying
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<UserRevocationMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();

app.MapReverseProxy();

app.Run();
