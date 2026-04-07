using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;
using MongoDB.Bson;

namespace GoingMy.Post.Application.Commands;

/// <summary>
/// Command to create a new post.
/// </summary>
public record CreatePostCommand(
    string Title,
    string Content,
    string UserId,
    string Username
) : IRequest<PostDto>;

/// <summary>
/// Handler for the CreatePostCommand.
/// </summary>
public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IPostRepository _postRepository;

    public CreatePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Domain.Entities.Post(
            id: ObjectId.GenerateNewId().ToString(),
            title: request.Title,
            content: request.Content,
            userId: request.UserId,
            username: request.Username,
            createdAt: DateTime.UtcNow
        );

        var createdPost = await _postRepository.AddAsync(post, cancellationToken);

        return new PostDto(
            Id: createdPost.Id,
            Title: createdPost.Title,
            Content: createdPost.Content,
            UserId: createdPost.UserId,
            Username: createdPost.Username,
            CreatedAt: createdPost.CreatedAt,
            UpdatedAt: createdPost.UpdatedAt
        );
    }
}
