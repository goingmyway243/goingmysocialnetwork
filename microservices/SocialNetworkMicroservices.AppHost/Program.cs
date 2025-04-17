var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var database = builder.AddPostgres("postgresql")
    .WithPgAdmin(containerName: "pgadmin");

builder.AddProject<Projects.SocialNetworkMicroservices_Identity>("identity")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
