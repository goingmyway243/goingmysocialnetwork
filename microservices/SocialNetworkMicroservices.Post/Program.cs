using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using OpenIddict.Validation.AspNetCore;
using SocialNetworkMicroservices.Post.Extensions;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Social Network - Identity API",
        Version = "v1",
        Description = "Identity microservice for the Social Network application. Provides OAuth 2.0/OpenID Connect authentication endpoints using OpenIddict.",
        Contact = new OpenApiContact
        {
            Name = "Social Network API Support",
            Email = "support@socialnetwork.com"
        }
    });

    // Add Bearer token authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: 'eyJhbGciOiJSUzI1NiIsImtpZCI6...'"
    });
});

// Configure OpenIddict validation
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(builder.Configuration.GetServiceUri("identity")!);
        options.AddAudiences("post_api");

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Post API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Post API Documentation";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
        options.EnableDeepLinking();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Posts endpoints
app.MapGet("/api/posts", [Authorize] (ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("sub")?.Value ?? "unknown";
    var username = user.FindFirst("name")?.Value ?? "unknown";

    var posts = new[]
    {
        new Post(1, $"First post by {username}", "This is the content of the first post", userId, DateTime.UtcNow.AddHours(-2)),
        new Post(2, $"Second post by {username}", "This is the content of the second post", userId, DateTime.UtcNow.AddHours(-1)),
        new Post(3, $"Latest post by {username}", "This is the content of the latest post", userId, DateTime.UtcNow)
    };

    return Results.Ok(new { userId, username, posts });
})
.WithName("GetPosts")
.RequireAuthorization();

app.MapGet("/api/posts/{id:int}", [Authorize] (int id, ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("sub")?.Value ?? "unknown";
    var username = user.FindFirst("name")?.Value ?? "unknown";

    var post = new Post(id, $"Post #{id} by {username}", $"This is the content of post #{id}", userId, DateTime.UtcNow);

    return Results.Ok(post);
})
.WithName("GetPostById")
.RequireAuthorization();

app.MapPost("/api/posts", [Authorize] (CreatePostRequest request, ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("sub")?.Value ?? "unknown";
    var username = user.FindFirst("name")?.Value ?? "unknown";

    var newPost = new Post(
        Random.Shared.Next(1000, 9999),
        request.Title,
        request.Content,
        userId,
        DateTime.UtcNow
    );

    return Results.Created($"/api/posts/{newPost.Id}", new { message = "Post created successfully", post = newPost });
})
.WithName("CreatePost")
.RequireAuthorization();

app.MapPut("/api/posts/{id:int}", [Authorize] (int id, CreatePostRequest request, ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("sub")?.Value ?? "unknown";

    var updatedPost = new Post(id, request.Title, request.Content, userId, DateTime.UtcNow);

    return Results.Ok(new { message = "Post updated successfully", post = updatedPost });
})
.WithName("UpdatePost")
.RequireAuthorization();

app.MapDelete("/api/posts/{id:int}", [Authorize] (int id, ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("sub")?.Value ?? "unknown";

    return Results.Ok(new { message = $"Post {id} deleted successfully by user {userId}" });
})
.WithName("DeletePost")
.RequireAuthorization();

app.Run();

record Post(int Id, string Title, string Content, string UserId, DateTime CreatedAt);
record CreatePostRequest(string Title, string Content);
