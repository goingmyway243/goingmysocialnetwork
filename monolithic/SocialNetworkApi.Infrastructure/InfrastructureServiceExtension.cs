using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SocialNetworkApi.Application.Common.Interfaces;
using SocialNetworkApi.Domain.Common;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;
using SocialNetworkApi.Infrastructure.Identity;
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
            services.AddMongoRepository<UserEntity>("users");
            services.AddMongoRepository<PostEntity>("posts");
            services.AddMongoRepository<LikeEntity>("likes");
            services.AddMongoRepository<CommentEntity>("comments");
            services.AddMongoRepository<ChatroomEntity>("chatrooms");
            services.AddMongoRepository<ChatMessageEntity>("chat_messages");
            services.AddMongoRepository<FriendshipEntity>("friendships");

            // Configure Storage
            var storageConnectionString = configuration.GetConnectionString("AzureBlobStorage");
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                services.AddScoped<IStorageService, LocalStorageService>(_ => new LocalStorageService(contentRootPath));
            }
            else
            {
                services.AddSingleton(new BlobServiceClient(storageConnectionString));
                services.AddScoped<IStorageService, AzureStorageService>();
            }

            // Configure MongoDB
            services.AddSingleton<IMongoClient>(sp => new MongoClient(configuration.GetConnectionString("MongoDB")));

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase("goingmysocial");
            });

            return services;
        }

        public static void AddMongoRepository<T>(this IServiceCollection services, string collectionName) where T : BaseEntity
        {
            services.AddScoped<IRepository<T>>(sp =>
            {
                var db = sp.GetRequiredService<IMongoDatabase>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                return new Repository<T>(db, httpContextAccessor, collectionName);
            });
        }
    }
}
