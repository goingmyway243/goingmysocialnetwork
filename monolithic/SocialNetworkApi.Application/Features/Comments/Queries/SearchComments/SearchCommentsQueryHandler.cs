using AutoMapper;
using MediatR;
using MongoDB.Driver;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Comments.Queries;

public class SearchCommentsQueryHandler : IRequestHandler<SearchCommentsQuery, PagedResultDto<CommentDto>>
{
    private readonly IRepository<CommentEntity> _commentRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public SearchCommentsQueryHandler(IRepository<CommentEntity> commentRepository, IRepository<UserEntity> userRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<CommentDto>> Handle(SearchCommentsQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = request.PagedRequest;
        var builder = Builders<CommentEntity>.Filter;
        var filter = builder.Eq(c => c.PostId, request.PostId);

        var totalCount = await _commentRepository.GetAll().CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var listComments = await _commentRepository.GetAll()
            .Find(filter)
            .Skip(pagedRequest.SkipCount)
            .Limit(pagedRequest.PageSize)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var userIds = listComments.Select(c => c.UserId).Distinct().ToList();
        var users = await _userRepository.FindAsync(u => userIds.Contains(u.Id));

        var listCommentDtos = listComments.Select(_mapper.Map<CommentDto>).ToList();
        listCommentDtos.ForEach(commentDto =>
        {
            var user = users.FirstOrDefault(u => u.Id == commentDto.UserId);
            if (user != null)
            {
                commentDto.User = _mapper.Map<UserDto>(user);
            }
        });

        return PagedResultDto<CommentDto>.Success(listCommentDtos).WithPage(pagedRequest.PageIndex, (int)totalCount);
    }
}
