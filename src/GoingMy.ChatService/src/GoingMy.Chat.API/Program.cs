using GoingMy.Chat.API.Hubs;
using GoingMy.Chat.Application.Consumers;
using GoingMy.Chat.Domain.Repositories;
using GoingMy.Chat.Infrastructure.Data;
using GoingMy.Chat.Infrastructure.Repositories;
using GoingMy.ServiceDefaults;
using GoingMy.Shared;
using GoingMy.Shared.Events;
using MassTransit;
using MongoDB.Driver;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;
using System.Reflection;

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

// ── MassTransit + Kafka (event consumers) ────────────────────
var kafkaBootstrapServers = builder.Configuration.GetConnectionString(SharedServices.Kafka)
    ?? "localhost:9092";

builder.Services.AddMassTransit(x =>
{
    x.AddRider(rider =>
    {
        rider.AddConsumer<UserUpdatedEventConsumer>();
        rider.AddConsumer<UserDeletedEventConsumer>();

        rider.UsingKafka((context, cfg) =>
        {
            cfg.Host(kafkaBootstrapServers);

            cfg.TopicEndpoint<UserUpdatedEvent>(
                SharedServices.KafkaTopics.UserUpdated,
                SharedServices.KafkaConsumerGroups.ChatService,
                e => e.ConfigureConsumer<UserUpdatedEventConsumer>(context));

            cfg.TopicEndpoint<UserDeletedEvent>(
                SharedServices.KafkaTopics.UserDeleted,
                SharedServices.KafkaConsumerGroups.ChatService,
                e => e.ConfigureConsumer<UserDeletedEventConsumer>(context));
        });
    });
});

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
