using GoingMy.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var postgresql = builder.AddPostgres(SharedServices.Postgresql)
    .WithImage("postgres", "17-alpine")
    .WithDataVolume("goingmysocial-postgresql")
    .WithPgAdmin(containerName: "pgadmin")
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgresql.AddDatabase(SharedServices.IdentityDb);
var userDb = postgresql.AddDatabase(SharedServices.UserDb);

var mongodb = builder.AddMongoDB(SharedServices.MongoDB)
    .WithImage("mongodb/mongodb-community-server", "5.0-ubuntu2204-slim")
    .WithMongoExpress(containerName: "mongo-express")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var postDb = mongodb.AddDatabase(SharedServices.PostDb);
var chatDb = mongodb.AddDatabase(SharedServices.ChatDb);

var identityService = builder.AddProject<Projects.GoingMy_Auth_API>(SharedServices.IdentityApi)
    .WithReference(database)
    .WaitFor(database);

var postService = builder.AddProject<Projects.GoingMy_Post_API>(SharedServices.PostApi)
    .WithReference(postDb)
    .WaitFor(identityService)
    .WaitFor(postDb)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

var chatService = builder.AddProject<Projects.GoingMy_Chat_API>(SharedServices.ChatApi)
    .WithReference(chatDb)
    .WaitFor(identityService)
    .WaitFor(chatDb)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

var userService = builder.AddProject<Projects.GoingMy_User_API>(SharedServices.UserApi)
    .WithReference(userDb)
    .WaitFor(identityService)
    .WaitFor(userDb)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

builder.AddProject<Projects.GoingMy_ApiGateway>("api-gateway")
    .WithReference(identityService)
    .WithReference(userService)
    .WithReference(postService)
    .WithReference(chatService)
    .WaitFor(identityService)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

builder.Build().Run();
