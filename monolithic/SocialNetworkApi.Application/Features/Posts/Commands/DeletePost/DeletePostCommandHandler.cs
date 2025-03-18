using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Posts.Commands;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, CommandResult<Guid>>
{
    private readonly IRepository<PostEntity> _postRepository;

    public DeletePostCommandHandler(IRepository<PostEntity> postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<CommandResult<Guid>> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id);
        if (post == null)
        {
            return CommandResult<Guid>.Failure("Post not found.");
        }

        await _postRepository.DeleteAsync(post);
        return CommandResult<Guid>.Success(post.Id);
    }
}
