using GoingMy.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var postgresql = builder.AddPostgres(SharedServices.Postgresql)
    .WithImage("postgres", "17-alpine")
    .WithDataVolume("goingmysocial-postgresql")
    .WithPgAdmin(containerName: "pgadmin")
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgresql.AddDatabase(SharedServices.IdentityDb);

var mongodb = builder.AddMongoDB(SharedServices.MongoDB)
    .WithImage("mongodb/mongodb-community-server", "5.0-ubuntu2204-slim")
    .WithMongoExpress(containerName: "mongo-express")
    .WithHttpEndpoint(27017, 27017)
    .WithLifetime(ContainerLifetime.Persistent);

var postDb = mongodb.AddDatabase(SharedServices.PostDb);

var identityService = builder.AddProject<Projects.GoingMy_Auth_API>(SharedServices.IdentityApi)
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.GoingMy_Post_API>(SharedServices.PostApi)
    .WithReference(postDb)
    .WaitFor(identityService)
    .WaitFor(postDb)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

builder.Build().Run();
