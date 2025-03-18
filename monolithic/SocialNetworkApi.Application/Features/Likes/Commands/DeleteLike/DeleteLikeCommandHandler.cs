using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;
using System;

namespace SocialNetworkApi.Application.Features.Likes.Commands;

public class DeleteLikeCommandHandler : IRequestHandler<DeleteLikeCommand, CommandResult<Guid>>
{
    private readonly IRepository<LikeEntity> _likeRepository;

    public DeleteLikeCommandHandler(IRepository<LikeEntity> likeRepository)
    {
        _likeRepository = likeRepository;
    }
    public async Task<CommandResult<Guid>> Handle(DeleteLikeCommand request, CancellationToken cancellationToken)
    {
        var like = await _likeRepository.GetByIdAsync(request.Id);
        if (like == null)
        {
            return CommandResult<Guid>.Failure("Like not found.");
        }

        await _likeRepository.DeleteAsync(like);
        return CommandResult<Guid>.Success(like.Id);
    }
}
