using GoingMy.Chat.Domain.Repositories;
using GoingMy.Chat.Infrastructure.Repositories;
using GoingMy.Chat.Infrastructure.Data;
using GoingMy.Chat.API.Hubs;
using MongoDB.Driver;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;
using GoingMy.Shared;
using System.Reflection;
using GoingMy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration.GetConnectionString(SharedServices.ChatDb)));

builder.Services.AddScoped<MongoDbContext>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return new MongoDbContext(client, SharedServices.ChatDb);
});

// Register MediatR
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblies(
        Assembly.GetExecutingAssembly(),
        typeof(GoingMy.Chat.Application.Commands.SendMessageCommand).Assembly
    )
);

// Register repositories
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

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
builder.Services.AddControllers();
builder.Services.AddSignalR();

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

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapServiceDefaults();

app.Run();
