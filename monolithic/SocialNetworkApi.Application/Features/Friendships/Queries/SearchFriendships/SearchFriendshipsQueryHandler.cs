using AutoMapper;
using MediatR;
using MongoDB.Driver;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Friendships.Queries;

public class SearchFriendshipsQueryHandler : IRequestHandler<SearchFriendshipsQuery, PagedResultDto<FriendshipDto>>
{
    private readonly IRepository<FriendshipEntity> _friendshipRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public SearchFriendshipsQueryHandler(
        IRepository<FriendshipEntity> friendshipRepository,
        IRepository<UserEntity> userRepository,
        IMapper mapper)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<FriendshipDto>> Handle(SearchFriendshipsQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = request.PagedRequest;
        var builder = Builders<FriendshipEntity>.Filter;
        var filter = request.ExcludeFriendshipMakeByUser 
            ? builder.Eq(p => p.FriendId, request.UserId)
            : builder.Or(
                builder.Eq(p => p.UserId, request.UserId),
                builder.Eq(p => p.FriendId, request.UserId)
            );

        if (request.FilterStatus.Count > 0)
        {
            filter &= builder.In(p => p.Status, request.FilterStatus);
        }

        var totalCount = await _friendshipRepository.GetAll().CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var friendships = await _friendshipRepository.GetAll()
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip(pagedRequest.SkipCount)
            .Limit(pagedRequest.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = friendships.Select(p => p.UserId == request.UserId ? p.FriendId : p.UserId);
        var users = await _userRepository.FindAsync(u => userIds.Contains(u.Id));

        var result = friendships.Select(_mapper.Map<FriendshipDto>).ToList();
        result.ForEach(fs =>
        {
            var userInfo = users.FirstOrDefault(u => u.Id == fs.UserId || u.Id == fs.FriendId);
            fs.User = _mapper.Map<UserDto>(userInfo);
        });

        return PagedResultDto<FriendshipDto>.Success(result)
            .WithPage(pagedRequest.PageIndex, (int)totalCount);
    }
}
