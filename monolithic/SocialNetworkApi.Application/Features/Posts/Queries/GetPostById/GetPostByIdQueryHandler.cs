using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Posts.Queries;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, QueryResult<PostDto>>
{
    private readonly IRepository<PostEntity> _postRepository;
    private readonly IMapper _mapper;

    public GetPostByIdQueryHandler(IRepository<PostEntity> postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<QueryResult<PostDto>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id);
        if (post == null)
        {
            return QueryResult<PostDto>.Failure("Post not found.");
        }

        return QueryResult<PostDto>.Success(_mapper.Map<PostDto>(post));
    }
}
