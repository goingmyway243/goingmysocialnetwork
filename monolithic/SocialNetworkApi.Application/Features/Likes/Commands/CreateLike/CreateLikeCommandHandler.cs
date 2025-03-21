using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Likes.Commands;

public class CreateLikeCommandHandler : IRequestHandler<CreateLikeCommand, CommandResultDto<LikeDto>>
{
    private readonly IRepository<LikeEntity> _likeRepository;
    private readonly IMapper _mapper;

    public CreateLikeCommandHandler(IRepository<LikeEntity> likeRepository, IMapper mapper)
    {
        _likeRepository = likeRepository;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<LikeDto>> Handle(CreateLikeCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == default || request.PostId == default)
        {
            return CommandResultDto<LikeDto>.Failure("Invalid data.");
        }

        var existingLikeInPost = await _likeRepository
            .FirstOrDefaultAsync(l => l.UserId == request.UserId && l.PostId == request.PostId);

        if (existingLikeInPost != null)
        {
            var existingLikeDto = _mapper.Map<LikeDto>(existingLikeInPost);
            return CommandResultDto<LikeDto>.Success(existingLikeDto);
        }

        var likeInPost = _mapper.Map<LikeEntity>(request);
        likeInPost.Id = Guid.NewGuid();

        await _likeRepository.InsertAsync(likeInPost);

        var result = _mapper.Map<LikeDto>(likeInPost);
        return CommandResultDto<LikeDto>.Success(result);
    }
}
