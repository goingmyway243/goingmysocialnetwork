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

        return MapToDto(createdPost);
    }

    internal static PostDto MapToDto(Domain.Entities.Post p) => new(
        Id: p.Id,
        Title: p.Title,
        Content: p.Content,
        UserId: p.UserId,
        Username: p.Username,
        Likes: p.Likes,
        Comments: p.Comments,
        Author: p.Author is null ? null : new UserDto(
            Id: p.Author.Id,
            UserName: p.Author.UserName,
            FirstName: p.Author.FirstName,
            LastName: p.Author.LastName,
            AvatarUrl: p.Author.AvatarUrl,
            IsVerified: p.Author.IsVerified),
        CreatedAt: p.CreatedAt,
        UpdatedAt: p.UpdatedAt);
}
