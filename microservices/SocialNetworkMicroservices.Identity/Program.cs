using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SocialNetworkMicroservices.Identity;
using SocialNetworkMicroservices.Identity.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Social Network - Identity API",
        Version = "v1",
        Description = "Identity microservice for the Social Network application. Provides OAuth 2.0/OpenID Connect authentication endpoints using OpenIddict.",
        Contact = new OpenApiContact
        {
            Name = "Social Network API Support",
            Email = "support@socialnetwork.com"
        }
    });

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri("/connect/token", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    { "admin", "Access to admin features" }
                }
            }
        }
    });
});

// Register the DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("OpenIddictDb");
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
        // Enable the token endpoint
        options.SetTokenEndpointUris("/connect/token");
        options.SetAuthorizationEndpointUris("/connect/authorize");
        options.SetUserinfoEndpointUris("/connect/userinfo");

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
               .EnableUserinfoEndpointPassthrough();

        // Register scopes
        options.RegisterScopes("email", "profile", "roles", "admin");
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddControllers();

// Register services
builder.Services.AddSingleton<IUserService, TestUserService>();
builder.Services.AddHttpClient();

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
