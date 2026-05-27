using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;
using MediatR;
using MongoDB.Bson;

namespace GoingMy.Post.Application.Commands;

/// <summary>
/// Command to create a new post.
/// </summary>
public record CreatePostCommand(
    string Content,
    string UserId,
    string Username,
    string? FirstName,
    string? LastName
) : IRequest<PostDto>;

/// <summary>
/// Handler for the CreatePostCommand.
/// </summary>
public class CreatePostCommandHandler(IPostRepository postRepository, IPublishEndpoint publishEndpoint)
    : IRequestHandler<CreatePostCommand, PostDto>
{
    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Domain.Entities.Post(
            id: ObjectId.GenerateNewId().ToString(),
            content: request.Content,
            userId: request.UserId,
            username: request.Username,
            createdAt: DateTime.UtcNow
        );

        // Seed the denormalized author snapshot at creation time.
        // UserUpdatedEvent will enrich/sync the remaining profile fields later.
        post.Author = new Domain.Entities.User
        {
            Id = request.UserId,
            UserName = request.Username,
            FirstName = string.IsNullOrWhiteSpace(request.FirstName) ? request.Username : request.FirstName,
            LastName = request.LastName ?? string.Empty
        };

        var createdPost = await postRepository.AddAsync(post, cancellationToken);

        await publishEndpoint.Publish(new PostCreatedEvent
        {
            PostId = createdPost.Id,
            UserId = createdPost.UserId,
            Username = createdPost.Username,
            Content = createdPost.Content,
            MediaAttachments = createdPost.MediaAttachments?.Select(m => new GoingMy.Shared.Events.MediaAttachmentInfo(m.FileId, m.Url, m.ContentType, m.Width, m.Height)).ToList() ?? [],
            CreatedAt = createdPost.CreatedAt
        }, cancellationToken);

        return MapToDto(createdPost);
    }

    internal static PostDto MapToDto(Domain.Entities.Post p) => new(
        Id: p.Id,
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
            AvatarUrl: null,
            IsVerified: false),
        CreatedAt: p.CreatedAt,
        UpdatedAt: p.UpdatedAt,
        MediaAttachments: p.MediaAttachments is null || p.MediaAttachments.Count == 0
            ? null
            : p.MediaAttachments
                .Select(m => new MediaAttachmentDto(m.FileId, m.Url, m.ContentType, m.Width, m.Height))
                .ToList());
}
