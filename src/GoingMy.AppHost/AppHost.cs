using GoingMy.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var postgresql = builder.AddPostgres(SharedServices.Postgresql)
    .WithImage("postgres", "17-alpine")
    .WithDataVolume("goingmysocial-postgresql")
    .WithPgAdmin(containerName: "pgadmin")
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgresql.AddDatabase(SharedServices.IdentityDb);

var identityService = builder.AddProject<Projects.GoingMy_Auth_API>(SharedServices.IdentityApi)
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.GoingMy_Post_API>(SharedServices.PostApi)
    .WithReference(database)
    .WaitFor(identityService)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

builder.Build().Run();
