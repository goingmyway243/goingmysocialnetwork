using GoingMy.Post.Application.Commands;
using GoingMy.Post.Application.Queries;
using GoingMy.Post.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register MediatR
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblies(
        typeof(Program).Assembly,
        typeof(CreatePostCommand).Assembly
    )
);

// Register repositories
builder.Services.AddScoped<IPostRepository, PostRepository>();

// Configure OpenIddict validation
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(builder.Configuration["OpenIddict:Issuer"]!);
        options.UseSystemNetHttp();
        options.UseAspNetCore();
        options.AddAudiences("social-api");
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedHosts"]!.Split(','))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Map controllers
app.MapControllers();

app.Run();
