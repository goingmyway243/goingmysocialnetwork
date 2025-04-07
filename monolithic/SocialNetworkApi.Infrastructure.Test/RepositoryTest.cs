using Microsoft.AspNetCore.Http;
using Mongo2Go;
using MongoDB.Driver;
using Moq;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Infrastructure.Repositories;
using System.Security.Cryptography;

namespace SocialNetworkApi.Infrastructure.Test
{
    public class RepositoryTest : IDisposable
    {
        private readonly IMongoDatabase _mongoDB;
        private readonly MongoDbRunner _mongoDbRunner;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        public RepositoryTest()
        {
            _mongoDbRunner = MongoDbRunner.Start();
            _mongoDB = new MongoClient("mongodb://localhost:27017").GetDatabase("goingmysocial_test");//localhost:27017");

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        }

        public void Dispose()
        {
            _mongoDB.Client.DropDatabase("goingmysocial_test");
            _mongoDbRunner.Dispose();
        }

        [Fact]
        public async Task InsertAsync_ValidUser_Success()
        {
            // Arrange
            var expectedUser = new UserEntity()
            {
                Id = Guid.NewGuid(),
                FullName = "Tester",
                Email = "test@yopmail.com",
                PasswordHash = RandomNumberGenerator.GetHexString(64)
            };

            var repository = new Repository<UserEntity>(_mongoDB, _httpContextAccessorMock.Object, "users");

            await repository.InsertAsync(expectedUser);

            var result = await _mongoDB.GetCollection<UserEntity>("users").Find(p => p.Id == expectedUser.Id).FirstOrDefaultAsync();

            var totalCount = await _mongoDB.GetCollection<UserEntity>("users").CountDocumentsAsync(_ => true);

            Assert.NotNull(result);
            Assert.IsType<UserEntity>(result);
            Assert.Equal(expectedUser.Id, result.Id);
            Assert.Equal(expectedUser.FullName, result.FullName);
            Assert.Equal(expectedUser.PasswordHash, result.PasswordHash);
            Assert.Equal(1, totalCount);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(200000)]
        public async Task UpdateAsync_ChangeTotalLikeCountInPost_Success(int newLikeCount)
        {
            // Arrange
            var postToUpdate = new PostEntity()
            {
                Id = Guid.NewGuid(),
                LikeCount = 10
            };

            await _mongoDB.GetCollection<PostEntity>("posts").InsertOneAsync(postToUpdate);

            var repository = new Repository<PostEntity>(_mongoDB, _httpContextAccessorMock.Object, "posts");

            // Act
            postToUpdate.LikeCount = newLikeCount;

            await repository.UpdateAsync(postToUpdate);

            var result = await _mongoDB.GetCollection<PostEntity>("posts").Find(p => p.Id == postToUpdate.Id).FirstOrDefaultAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PostEntity>(result);
            Assert.Equal(postToUpdate.Id, result.Id);
            Assert.Equal(newLikeCount, result.LikeCount);
        }
    }
}
