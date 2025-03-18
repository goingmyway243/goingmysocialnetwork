using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Posts.Commands;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, CommandResult<PostDto>>
{
    private readonly IRepository<PostEntity> _postRepository;
    private readonly IMapper _mapper;

    public CreatePostCommandHandler(IRepository<PostEntity> postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<CommandResult<PostDto>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == default)
        {
            return CommandResult<PostDto>.Failure("Invalid user!");
        }

        var post = _mapper.Map<PostEntity>(request);
        post.Id = Guid.NewGuid();

        await _postRepository.InsertAsync(post);
        return CommandResult<PostDto>.Success(_mapper.Map<PostDto>(post));
    }
}
