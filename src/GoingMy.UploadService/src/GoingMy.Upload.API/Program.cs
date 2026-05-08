using GoingMy.Upload.Application.Extensions;
using GoingMy.Upload.Application.Consumers;
using GoingMy.Upload.Application.Saga;
using GoingMy.Upload.Domain.Repositories;
using GoingMy.Upload.Domain.Storage;
using GoingMy.Upload.Infrastructure.Data;
using GoingMy.Upload.Infrastructure.Repositories;
using GoingMy.Upload.Infrastructure.Storage;
using GoingMy.Upload.Infrastructure.Workers;
using GoingMy.ServiceDefaults;
using GoingMy.Shared;
using MassTransit;
using MongoDB.Driver;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// MongoDB
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration.GetConnectionString(SharedServices.UploadDb)));

builder.Services.AddScoped<UploadDbContext>(sp =>
    new UploadDbContext(sp.GetRequiredService<IMongoClient>(), SharedServices.UploadDb));

// Storage
builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection("Storage"));
builder.Services.AddSingleton<IFileStorageProvider, LocalFileStorageProvider>();

// Application
builder.Services.AddUploadApplicationServices();

// Repositories
builder.Services.AddScoped<IMediaFileRepository, MediaFileRepository>();

// Background worker
builder.Services.AddHostedService<OrphanedMediaCleanupWorker>();

// MassTransit + RabbitMQ + Saga
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<PostMediaStateMachine, PostMediaSagaState>()
        .MongoDbRepository(r =>
        {
            r.Connection = builder.Configuration.GetConnectionString(SharedServices.UploadDb)!;
            r.DatabaseName = SharedServices.UploadDb;
        });

    x.AddConsumer<ValidateMediaConsumer>();
    x.AddConsumer<FileOrphanedEventConsumer>();

    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration.GetConnectionString(SharedServices.RabbitMQ)!));

        cfg.ReceiveEndpoint("post-media-saga", e =>
            e.ConfigureSaga<PostMediaSagaState>(context));

        cfg.ReceiveEndpoint("upload-validate-media", e =>
            e.ConfigureConsumer<ValidateMediaConsumer>(context));

        cfg.ReceiveEndpoint("upload-file-orphaned", e =>
            e.ConfigureConsumer<FileOrphanedEventConsumer>(context));

        cfg.ConfigureEndpoints(context);
    });
});

// Auth
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

// Allow large multipart uploads
builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = 440 * 1024 * 1024);

var app = builder.Build();

// Initialize MongoDB indexes
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<UploadDbContext>();
    await ctx.InitializeAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

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
