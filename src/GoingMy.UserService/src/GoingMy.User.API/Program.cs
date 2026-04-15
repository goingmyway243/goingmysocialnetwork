using GoingMy.ServiceDefaults;
using GoingMy.Shared;
using GoingMy.Shared.Events;
using GoingMy.User.Application.Extensions;
using GoingMy.User.Domain.Repositories;
using GoingMy.User.Infrastructure.Data;
using GoingMy.User.Infrastructure.Repositories;
using GoingMy.User.Infrastructure.Workers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// ── Outbox Interceptor ────────────────────────────────────────
builder.Services.AddSingleton<UserProfileOutboxInterceptor>();

// ── Database ──────────────────────────────────────────────────
builder.Services.AddDbContext<UserDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString(SharedServices.UserDb);
    options.UseNpgsql(connectionString);
    options.AddInterceptors(sp.GetRequiredService<UserProfileOutboxInterceptor>());
});

// ── Application services (MediatR) ───────────────────────────
builder.Services.AddUserApplicationServices();

// ── Repositories ─────────────────────────────────────────────
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserFollowRepository, UserFollowRepository>();

// ── MassTransit + Kafka (Outbox publisher) ───────────────────
var kafkaBootstrapServers = builder.Configuration.GetConnectionString(SharedServices.Kafka)
    ?? "localhost:9092";

builder.Services.AddMassTransit(x =>
{
    x.AddRider(rider =>
    {
        rider.AddProducer<UserCreatedEvent>(SharedServices.KafkaTopics.UserCreated);
        rider.AddProducer<UserUpdatedEvent>(SharedServices.KafkaTopics.UserUpdated);
        rider.AddProducer<UserDeletedEvent>(SharedServices.KafkaTopics.UserDeleted);

        rider.UsingKafka((_, cfg) =>
        {
            cfg.Host(kafkaBootstrapServers);
        });
    });
});

// ── Outbox publisher background worker ───────────────────────
builder.Services.AddHostedService<OutboxPublisherWorker>();

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
app.UseGatewayAuthentication();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapServiceDefaults();

app.Run();
