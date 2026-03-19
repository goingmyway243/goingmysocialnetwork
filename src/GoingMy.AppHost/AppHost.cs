var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL resource (shared database for all services)
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

// Auth Service
var authServiceDb = postgres.AddDatabase("AuthDb");
var authService = builder.AddProject<Projects.GoingMy_Auth_API>("auth-api")
    .WithReference(authServiceDb)
    .WaitFor(authServiceDb);

// Post Service
var postServiceDb = postgres.AddDatabase("PostDb");
var postService = builder.AddProject<Projects.GoingMy_Post_API>("post-api")
    .WithReference(postServiceDb)
    .WaitFor(postServiceDb);

builder.Build().Run();
