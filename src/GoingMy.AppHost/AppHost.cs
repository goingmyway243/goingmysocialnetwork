var builder = DistributedApplication.CreateBuilder(args);

var postgresql = builder.AddPostgres("postgresql")
    .WithImage("postgres", "17-alpine")
    .WithDataVolume("aspire_postgresql")
    .WithPgAdmin(containerName: "pgadmin")
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgresql.AddDatabase("goingmysocial-identity-db");

var identityService = builder.AddProject<Projects.GoingMy_Auth_API>("identity")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.GoingMy_Post_API>("post")
    .WithReference(identityService)
    .WithReference(database)
    .WaitFor(identityService)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

builder.Build().Run();
