using System.Net;
using System.Net.Http.Json;
using GoingMy.Auth.API.Data;
using GoingMy.Auth.API.Dtos;
using GoingMy.Auth.API.Enums;
using GoingMy.Auth.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using MassTransit;
using System.Text.Json;
namespace GoingMy.Auth.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Override external connection strings before Program.cs builds external clients.
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RabbitMQ"] = "amqp://guest:guest@localhost:5672",
                ["Redis:AbortOnConnectFail"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove real DbContext/provider registrations from Program.cs
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            // Add in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
                options.UseOpenIddict();
            });

            // Mock MassTransit IPublishEndpoint
            var publishEndpointMock = new Mock<IPublishEndpoint>();
            publishEndpointMock
                .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            services.AddSingleton(publishEndpointMock.Object);

            // Configure Identity
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;

                // User settings
                options.User.RequireUniqueEmail = true;

                // SignIn settings (optional)
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            // Create service provider to seed database
            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed test user
            var testUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test-email@example.com",
                FirstName = "Test",
                LastName = "User",
                Roles = new List<UserRole> { UserRole.User },
            };

            userManager.CreateAsync(testUser, "Password@123").Wait();
        });
    }
}

public class AuthorizationFlowTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthorizationFlowTest(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SignUp_ShouldReturnSuccess()
    {
        // Arrange
        var signUpRequest = new SignUpRequest
        {
            Username = Guid.NewGuid().ToString("N").Substring(0, 8), // Unique username
            Password = "Password@123",
            Email = $"{Guid.NewGuid()}@example.com", // Unique email to avoid conflicts
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user/signup", signUpRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        var applicationUser = JsonSerializer.Deserialize<ApplicationUser>(responseContent, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected status code 201 Created, but got {response.StatusCode}. Response: {responseContent}");

        Assert.NotNull(applicationUser);
        Assert.Contains(signUpRequest.Username, applicationUser.UserName, StringComparison.OrdinalIgnoreCase);
    }
}
