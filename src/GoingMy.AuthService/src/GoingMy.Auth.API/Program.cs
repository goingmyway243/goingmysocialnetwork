using GoingMy.Auth.API.Data;
using GoingMy.Auth.API.Models;
using GoingMy.Auth.API.Services;
using GoingMy.ServiceDefaults;
using GoingMy.Shared;
using GoingMy.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using GoingMy.Auth.API.Components;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Add Blazor Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAntiforgery();

builder.Services.AddHttpClient();

// Identity cookie authentication for the login session (used by PKCE flow)
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options => options.LoginPath = "/login")
    .AddCookie(IdentityConstants.ExternalScheme)
    .AddCookie(IdentityConstants.TwoFactorUserIdScheme);

builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(policy =>
  {
      policy.WithOrigins(builder.Configuration["CorsOrigins"]!.Split(','))
            .AllowAnyHeader()
            .AllowAnyMethod();
  });
});

// Register the DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
  var connectionString = builder.Configuration.GetConnectionString(SharedServices.IdentityDb);
  options.UseNpgsql(connectionString);
  options.UseOpenIddict();
});

// Register OpenIddict services
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
      options.UseEntityFrameworkCore()
             .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
      // Set the issuer
      options.SetIssuer(new Uri(builder.Configuration["OpenIddict:Issuer"] ?? "https://localhost:7001"));

      // Configure OpenIddict server options
      /*var encryptionKey = builder.Configuration["OpenIddict:Key"] ?? throw new Exception("OpenIddict key is not configured.");
      options.AddEncryptionKey(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encryptionKey)));*/
      options.SetAccessTokenLifetime(TimeSpan.FromMinutes(int.Parse(builder.Configuration["OpenIddict:AccessTokenLifetime"]!)));
      options.SetRefreshTokenLifetime(TimeSpan.FromMinutes(int.Parse(builder.Configuration["OpenIddict:RefreshTokenLifetime"]!)));

      // Enable the token endpoint
      options.SetTokenEndpointUris("/connect/token");
      options.SetAuthorizationEndpointUris("/connect/authorize");
      options.SetUserinfoEndpointUris("/connect/userinfo");
      options.SetLogoutEndpointUris("/connect/logout");

      // Enable the flows
      options.AllowPasswordFlow();
      options.AllowClientCredentialsFlow();
      options.AllowRefreshTokenFlow();
      options.AllowAuthorizationCodeFlow();

      // Register the signing and encryption credentials
      options.AddDevelopmentEncryptionCertificate()
             .AddDevelopmentSigningCertificate();

      // Register the ASP.NET Core host and configure the ASP.NET Core options
      options.UseAspNetCore()
             .EnableTokenEndpointPassthrough()
             .EnableAuthorizationEndpointPassthrough()
             .EnableUserinfoEndpointPassthrough()
             .EnableLogoutEndpointPassthrough();

      options.DisableAccessTokenEncryption();

      options.RegisterScopes("social_api", "email", "profile", "roles", "openid", "offline_access");
    })
    .AddValidation(options =>
    {
      options.UseLocalServer();
      options.UseAspNetCore();
    });

builder.Services.AddControllers();

// Register ASP.NET Core Identity services
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
  // Password settings
  options.Password.RequireDigit = true;
  options.Password.RequireLowercase = true;
  options.Password.RequireUppercase = true;
  options.Password.RequiredLength = 6;
  options.Password.RequireNonAlphanumeric = false;

  // User settings
  options.User.RequireUniqueEmail = true;

  // SignIn settings (optional)
  options.SignIn.RequireConfirmedEmail = false;
  options.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Register custom services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRefreshTokenBlacklistService, RefreshTokenBlacklistService>();
builder.Services.AddScoped<IUserRevocationService, UserRevocationService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Register Redis connection (Aspire provides connection string via configuration)
var redisConnectionString = builder.Configuration.GetConnectionString(SharedServices.Redis) ?? "localhost:6379";
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// ── MassTransit + RabbitMQ (event publisher) ─────────────────────
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration.GetConnectionString(SharedServices.RabbitMQ)!));
        cfg.ConfigureEndpoints(context);
    });
});

// Build the app
var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
  var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
  dbContext.Database.EnsureCreated();
  await OpenIddictSeeder.SeedAsync(scope.ServiceProvider);
  await UserSeeder.SeedUsersAsync(scope.ServiceProvider);
}

app.UseCors();
app.UseHttpsRedirection();

// Map static files (CSS, JS, etc.) - must come before routing
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Map Blazor components for the UI
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference();
}

app.MapControllers();
app.MapServiceDefaults();

app.Run();
