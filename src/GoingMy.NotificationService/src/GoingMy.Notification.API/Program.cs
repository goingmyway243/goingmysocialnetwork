using GoingMy.Notification.API.Hubs;
using GoingMy.Notification.API.Services;
using GoingMy.Notification.Application.Abstractions;
using GoingMy.Notification.Application.Consumers;
using GoingMy.Notification.Application.Extensions;
using GoingMy.Notification.Domain.Repositories;
using GoingMy.Notification.Infrastructure.Data;
using GoingMy.Notification.Infrastructure.Repositories;
using GoingMy.ServiceDefaults;
using GoingMy.Shared;
using GoingMy.Shared.Events;
using MassTransit;
using MongoDB.Driver;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration.GetConnectionString(SharedServices.NotificationDb)));

builder.Services.AddScoped<MongoDbContext>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return new MongoDbContext(client, SharedServices.NotificationDb);
});

// Register Application services (MediatR)
builder.Services.AddNotificationApplicationServices();

// Register repository
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Register SignalR push service (implements abstraction from Application layer)
builder.Services.AddScoped<INotificationPushService, NotificationPushService>();

// ── MassTransit + RabbitMQ (event consumers) ────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PostLikedNotificationConsumer>();
    x.AddConsumer<CommentAddedNotificationConsumer>();
    x.AddConsumer<UserFollowedNotificationConsumer>();
    x.AddConsumer<PostWithMediaSagaCompletedNotificationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration.GetConnectionString(SharedServices.RabbitMQ)!));

        cfg.ReceiveEndpoint($"{nameof(PostLikedEvent)}_notification_consumer", e =>
            e.ConfigureConsumer<PostLikedNotificationConsumer>(context));

        cfg.ReceiveEndpoint($"{nameof(CommentAddedEvent)}_notification_consumer", e =>
            e.ConfigureConsumer<CommentAddedNotificationConsumer>(context));

        cfg.ReceiveEndpoint($"{nameof(UserFollowedEvent)}_notification_consumer", e =>
            e.ConfigureConsumer<UserFollowedNotificationConsumer>(context));

        cfg.ReceiveEndpoint($"{nameof(PostWithMediaSagaCompletedEvent)}_notification_consumer", e =>
            e.ConfigureConsumer<PostWithMediaSagaCompletedNotificationConsumer>(context));

        cfg.ConfigureEndpoints(context);
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
app.MapHub<NotificationHub>("/hubs/notification");
app.MapServiceDefaults();

app.Run();
