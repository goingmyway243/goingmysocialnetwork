using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Commands;

/// <summary>
/// Command to update an existing post.
/// </summary>
public record UpdatePostCommand(
    string Id,
    string Title,
    string Content,
    string UserId
) : IRequest<PostDto>;

/// <summary>
/// Handler for the UpdatePostCommand.
/// </summary>
public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, PostDto>
{
    private readonly IPostRepository _postRepository;

    public UpdatePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostDto> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Post with ID {request.Id} not found");

        // Verify ownership
        if (post.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only update your own posts");
        }

        post.Update(request.Title, request.Content);
        var updatedPost = await _postRepository.UpdateAsync(post, cancellationToken);

        return new PostDto(
            Id: updatedPost.Id,
            Title: updatedPost.Title,
            Content: updatedPost.Content,
            UserId: updatedPost.UserId,
            Username: updatedPost.Username,
            CreatedAt: updatedPost.CreatedAt,
            UpdatedAt: updatedPost.UpdatedAt
        );
    }
}
