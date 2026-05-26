using GoingMy.Post.Application.Dtos;
using GoingMy.Shared.Events;
using MassTransit;
using MediatR;

namespace GoingMy.Post.Application.Commands;

public record CreatePostWithMediaCommand(
    string Content,
    string UserId,
    string Username,
    IReadOnlyList<string> MediaFileIds,
    string? FirstName,
    string? LastName
) : IRequest<PostDto>;

public class CreatePostWithMediaCommandHandler(IPublishEndpoint publishEndpoint)
    : IRequestHandler<CreatePostWithMediaCommand, PostDto>
{
    public async Task<PostDto> Handle(CreatePostWithMediaCommand request, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid();

        // Publish the saga initiation event
        await publishEndpoint.Publish(new PostWithMediaRequestedEvent
        {
            CorrelationId = correlationId,
            UserId = request.UserId,
            Username = request.Username,
            Content = request.Content,
            MediaFileIds = request.MediaFileIds,
            FirstName = request.FirstName,
            LastName = request.LastName
        }, ct);

        // Return a placeholder response with pending status
        // In a real system, you'd track the saga and return when complete,
        // or use SignalR to notify the client when the post is ready
        return new PostDto(
            Id: $"pending-{correlationId}",
            Content: request.Content,
            UserId: request.UserId,
            Username: request.Username,
            Likes: 0,
            Comments: 0,
            Author: new UserDto(
                Id: request.UserId,
                UserName: request.Username,
                FirstName: string.IsNullOrWhiteSpace(request.FirstName) ? request.Username : request.FirstName,
                LastName: request.LastName ?? string.Empty,
                AvatarUrl: null,
                IsVerified: false),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null);
    }
}
