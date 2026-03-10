var builder = DistributedApplication.CreateBuilder(args);

var postgresql = builder.AddPostgres("postgresql")
    .WithImage("postgres", "17-alpine")
    .WithDataVolume("postgresql")
    .WithPgAdmin(containerName: "pgadmin")
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgresql.AddDatabase("goingmysocial-identity-db");

var identityService = builder.AddProject<Projects.SocialNetworkMicroservices_Identity>("identity")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.SocialNetworkMicroservices_Post>("post")
    .WithReference(identityService)
    .WithReference(database);

builder.Build().Run();
