using GoingMy.Shared;
using GoingMy.User.Application.Extensions;
using GoingMy.User.Domain.Repositories;
using GoingMy.User.Infrastructure.Data;
using GoingMy.User.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// ── Database ──────────────────────────────────────────────────
builder.Services.AddDbContext<UserDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString(SharedServices.UserDb);
    options.UseNpgsql(connectionString);
});

// ── Application services (MediatR) ───────────────────────────
builder.Services.AddUserApplicationServices();

// ── Repositories ─────────────────────────────────────────────
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserFollowRepository, UserFollowRepository>();

// ── OpenIddict token validation ───────────────────────────────
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(builder.Configuration["OpenIddict:Issuer"]!);
        options.UseSystemNetHttp();
        options.UseAspNetCore();
        options.AddAudiences("social-api");
    });

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

builder.Services.AddControllers();

var app = builder.Build();

// ── Apply EF Core migrations on startup ──────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.Run();
