using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Likes.Commands.CreateLike;

public class CreateLikeCommandHandler : IRequestHandler<CreateLikeCommand, CommandResult<LikeDto>>
{
    private readonly IRepository<LikeEntity> _likeRepository;
    private readonly IMapper _mapper;

    public CreateLikeCommandHandler(IRepository<LikeEntity> likeRepository, IMapper mapper)
    {
        _likeRepository = likeRepository;
        _mapper = mapper;
    }

    public async Task<CommandResult<LikeDto>> Handle(CreateLikeCommand request, CancellationToken cancellationToken)
    {
        var existingLikeInPost = await _likeRepository
            .FirstOrDefaultAsync(l => l.UserId == request.UserId && l.PostId == request.PostId);

        if (existingLikeInPost != null)
        {
            var existingLikeDto = _mapper.Map<LikeDto>(existingLikeInPost);
            return CommandResult<LikeDto>.Success(existingLikeDto);
        }

        var likeInPost = _mapper.Map<LikeEntity>(request);
        likeInPost.Id = Guid.NewGuid();

        await _likeRepository.InsertAsync(likeInPost);

        var result = _mapper.Map<LikeDto>(likeInPost);
        return CommandResult<LikeDto>.Success(result);
    }
}
