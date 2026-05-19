using Elastic.Clients.Elasticsearch;
using GoingMy.Search.API.Consumers;
using GoingMy.Search.API.Infrastructure;
using GoingMy.Search.API.Services;
using GoingMy.ServiceDefaults;
using GoingMy.Shared;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["OpenIddict:Issuer"]!;
        options.Audience = "social-api";
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var esUri = builder.Configuration.GetConnectionString(SharedServices.Elasticsearch);
    var settings = new ElasticsearchClientSettings(new Uri(esUri!));
    return new ElasticsearchClient(settings);
});

builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddHostedService<ElasticsearchInitializer>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration.GetConnectionString(SharedServices.RabbitMQ)!));

        cfg.ReceiveEndpoint("search-post-events", e =>
            e.ConfigureConsumer<PostEventConsumer>(context));

        cfg.ReceiveEndpoint("search-user-events", e =>
            e.ConfigureConsumer<UserEventConsumer>(context));

        cfg.ReceiveEndpoint("search-post-interactions", e =>
            e.ConfigureConsumer<PostInteractionConsumer>(context));

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

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
