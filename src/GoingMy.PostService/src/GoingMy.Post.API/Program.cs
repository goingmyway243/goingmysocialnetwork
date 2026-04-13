using GoingMy.Post.Domain.Repositories;
using GoingMy.Post.Infrastructure.Repositories;
using GoingMy.Post.Infrastructure.Data;
using GoingMy.Post.Application.Extensions;
using MongoDB.Driver;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;
using GoingMy.Shared;
using GoingMy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(builder.Configuration.GetConnectionString(SharedServices.PostDb)));

builder.Services.AddScoped<MongoDbContext>(provider =>
{
  var client = provider.GetRequiredService<IMongoClient>();
  return new MongoDbContext(client, SharedServices.PostDb);
});

// Register Post.Application services (includes MediatR)
builder.Services.AddPostApplicationServices();

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

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Initialize MongoDB indexes
using (var scope = app.Services.CreateScope())
{
  var mongoDbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
  await mongoDbContext.InitializeAsync();
}

app.UseHttpsRedirection();

app.UseGatewayAuthentication();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference();
}

// Map controllers
app.MapControllers();
app.MapServiceDefaults();

app.Run();
