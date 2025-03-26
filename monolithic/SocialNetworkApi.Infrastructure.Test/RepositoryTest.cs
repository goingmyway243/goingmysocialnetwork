using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Infrastructure.Persistence;
using SocialNetworkApi.Infrastructure.Repositories;
using System.Security.Cryptography;

namespace SocialNetworkApi.Infrastructure.Test
{
    public class RepositoryTest
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        public RepositoryTest()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("testdb")
                .Options;

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
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

            var dbContext = CreateDbContext();
            var repository = new Repository<UserEntity>(dbContext, _httpContextAccessorMock.Object);

            await repository.InsertAsync(expectedUser);

            var result = await dbContext.Set<UserEntity>().FindAsync(expectedUser.Id);

            var totalCount = await dbContext.Set<UserEntity>().CountAsync();

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

            var dbContext = CreateDbContext();
            dbContext.Add(postToUpdate);
            await dbContext.SaveChangesAsync();

            var repository = new Repository<PostEntity>(dbContext, _httpContextAccessorMock.Object);

            // Act
            postToUpdate.LikeCount = newLikeCount;
            
            await repository.UpdateAsync(postToUpdate);

            var result = dbContext.Set<PostEntity>().Find(postToUpdate.Id);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PostEntity>(result);
            Assert.Equal(postToUpdate.Id, result.Id);
            Assert.Equal(newLikeCount, result.LikeCount);
        }

        private ApplicationDbContext CreateDbContext()
        {
            var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            return context;
        }
    }
}
