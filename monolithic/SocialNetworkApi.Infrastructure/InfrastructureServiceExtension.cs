using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetworkApi.Application.Common.Interfaces;
using SocialNetworkApi.Domain.Interfaces;
using SocialNetworkApi.Infrastructure.Identity;
using SocialNetworkApi.Infrastructure.Persistence;
using SocialNetworkApi.Infrastructure.Repositories;
using SocialNetworkApi.Infrastructure.Storage;

namespace SocialNetworkApi.Infrastructure
{
    public static class InfrastructureServiceExtension
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            string contentRootPath)
        {
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Configure Storage
            var storageConnectionString = configuration.GetConnectionString("AzureBlobStorage");
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                services.AddScoped<IStorageService, LocalStorageService>();
            }
            else
            {
                services.AddSingleton(new BlobServiceClient(storageConnectionString));
                services.AddScoped<IStorageService, AzureStorageService>();
            }

            // Configure MySQL
            var connectionString = configuration.GetConnectionString("MySQLConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
#if DEBUG
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
#endif
            );

            return services;
        }

        public static IServiceProvider InitialzeDatabase(this IServiceProvider serviceProvider)
        {
            // Seed the database
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated(); // Create the database if it doesn't exist
            }

            return serviceProvider;
        }
    }
}
