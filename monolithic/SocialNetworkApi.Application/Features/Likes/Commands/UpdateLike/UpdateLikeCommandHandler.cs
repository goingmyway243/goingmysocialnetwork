using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Likes.Commands;

public class UpdateLikeCommandHandler : IRequestHandler<UpdateLikeCommand, CommandResultDto<LikeDto>>
{
    private readonly IRepository<LikeEntity> _likeRepository;
    private readonly IMapper _mapper;

    public UpdateLikeCommandHandler(IRepository<LikeEntity> likeRepository, IMapper mapper)
    {
        _likeRepository = likeRepository;
        _mapper = mapper;
    }

    public Task<CommandResultDto<LikeDto>> Handle(UpdateLikeCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
