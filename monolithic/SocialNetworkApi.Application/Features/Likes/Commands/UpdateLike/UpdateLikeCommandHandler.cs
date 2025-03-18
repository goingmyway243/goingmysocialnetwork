using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Likes.Commands.UpdateLike;

public class UpdateLikeCommandHandler : IRequestHandler<UpdateLikeCommand, CommandResult<LikeDto>>
{
    private readonly IRepository<LikeEntity> _likeRepository;
    private readonly IMapper _mapper;

    public UpdateLikeCommandHandler(IRepository<LikeEntity> likeRepository, IMapper mapper)
    {
        _likeRepository = likeRepository;
        _mapper = mapper;
    }

    public Task<CommandResult<LikeDto>> Handle(UpdateLikeCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
