var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgresql = builder.AddPostgres("postgresql")
    .WithImage("postgres", "17-alpine")
    .WithDataVolume("postgresql")
    .WithPgAdmin(containerName: "pgadmin");

var database = postgresql.AddDatabase("goingmysocial-identity-db");

var identity = builder.AddProject<Projects.SocialNetworkMicroservices_Identity>("identity")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.SocialNetworkMicroservices_Post>("post")
    .WithReference(database)
    .WithReference(cache)
    .WithReference(identity)
    .WaitFor(identity);

builder.Build().Run();
