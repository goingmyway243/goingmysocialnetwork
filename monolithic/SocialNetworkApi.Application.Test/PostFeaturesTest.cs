using AutoMapper;
using Moq;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Common.Interfaces;
using SocialNetworkApi.Application.Features.Posts.Commands;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Test;

public class PostFeaturesTest
{
    private readonly Mock<IRepository<PostEntity>> _postRepositoryMock;
    private readonly Mock<IRepository<UserEntity>> _userRepositoryMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    public PostFeaturesTest()
    {
        _postRepositoryMock = new Mock<IRepository<PostEntity>>();
        _userRepositoryMock = new Mock<IRepository<UserEntity>>();
        _storageServiceMock = new Mock<IStorageService>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task CreatePost_ValidInput_ShouldReturnPost()
    {
        // Arrange
        var request = new CreatePostCommand()
        {
            Caption = "Hello",
            UserId = Guid.NewGuid(),
        };
        var entity = new PostEntity()
        {
            UserId = request.UserId,
            Caption = request.Caption
        };
        var expectedPostDto = new PostDto
        {
            Caption = request.Caption,
            UserId = request.UserId
        };

        _mapperMock.Setup(m => m.Map<PostEntity>(request)).Returns(entity);
        _mapperMock.Setup(m => m.Map<PostDto>(entity)).Returns(expectedPostDto);

        var handler = new CreatePostCommandHandler(
            _postRepositoryMock.Object, 
            _userRepositoryMock.Object,
            _storageServiceMock.Object,
            _mapperMock.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedPostDto.Caption, result.Data.Caption);
        Assert.Equal(expectedPostDto.UserId, result.Data.UserId);
    }
}
