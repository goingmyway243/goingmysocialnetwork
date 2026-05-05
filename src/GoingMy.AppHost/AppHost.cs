using GoingMy.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var postgresql = builder.AddPostgres(SharedServices.Postgresql)
    .WithImage("postgres", "17-alpine")
    .WithDataVolume("goingmysocial-postgresql")
    .WithPgAdmin(containerName: "pgadmin")
    .WithLifetime(ContainerLifetime.Persistent);

var identityDb = postgresql.AddDatabase(SharedServices.IdentityDb);
var userDb = postgresql.AddDatabase(SharedServices.UserDb);

var mongodb = builder.AddMongoDB(SharedServices.MongoDB)
    .WithImage("mongodb/mongodb-community-server", "5.0-ubuntu2204-slim")
    .WithMongoExpress(containerName: "mongo-express")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var postDb = mongodb.AddDatabase(SharedServices.PostDb);
var chatDb = mongodb.AddDatabase(SharedServices.ChatDb);

var rabbitmq = builder.AddRabbitMQ(SharedServices.RabbitMQ)
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var redis = builder.AddRedis(SharedServices.Redis)
    .WithImage("redis", "7-alpine")
    .WithRedisInsight(containerName: "redis-insight")
    .WithDataVolume("goingmysocial-redis")
    .WithLifetime(ContainerLifetime.Persistent);

var identityService = builder.AddProject<Projects.GoingMy_Auth_API>(SharedServices.IdentityApi)
    .WithReference(identityDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(identityDb)
    .WaitFor(redis)
    .WaitFor(rabbitmq);

var postService = builder.AddProject<Projects.GoingMy_Post_API>(SharedServices.PostApi)
    .WithReference(postDb)
    .WithReference(rabbitmq)
    .WaitFor(identityService)
    .WaitFor(postDb)
    .WaitFor(rabbitmq)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

var userService = builder.AddProject<Projects.GoingMy_User_API>(SharedServices.UserApi)
    .WithReference(userDb)
    .WithReference(rabbitmq)
    .WaitFor(identityService)
    .WaitFor(userDb)
    .WaitFor(rabbitmq)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

var chatService = builder.AddProject<Projects.GoingMy_Chat_API>(SharedServices.ChatApi)
    .WithReference(chatDb)
    .WithReference(rabbitmq)
    .WithReference(userService)
    .WaitFor(identityService)
    .WaitFor(chatDb)
    .WaitFor(rabbitmq)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

builder.AddProject<Projects.GoingMy_ApiGateway>("api-gateway")
    .WithReference(identityService)
    .WithReference(userService)
    .WithReference(postService)
    .WithReference(chatService)
    .WithReference(redis)
    .WaitFor(identityService)
    .WaitFor(redis)
    .WithEnvironment("OpenIddict:Issuer", identityService.GetEndpoint("https"));

builder.Build().Run();
