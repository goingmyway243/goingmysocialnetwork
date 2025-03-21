using AutoMapper;
using Moq;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Features.Posts.Commands;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Test;

public class PostFeaturesTest
{
    private readonly Mock<IRepository<PostEntity>> _postRepository;
    private readonly Mock<IMapper> _mapper;

    public PostFeaturesTest()
    {
        _postRepository = new Mock<IRepository<PostEntity>>();
        _mapper = new Mock<IMapper>();
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

        _mapper.Setup(m => m.Map<PostEntity>(request)).Returns(entity);
        _mapper.Setup(m => m.Map<PostDto>(entity)).Returns(expectedPostDto);

        var handler = new CreatePostCommandHandler(_postRepository.Object, _mapper.Object);

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
