using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var searchFriendshipQuery = _friendshipRepository.GetAll();
        searchFriendshipQuery = request.ExcludeFriendshipMakeByUser
            ? searchFriendshipQuery.Where(p => p.FriendId == request.UserId)
            : searchFriendshipQuery.Where(p => p.UserId == request.UserId || p.FriendId == request.UserId);

        if (request.FilterStatus.Count > 0)
        {
            searchFriendshipQuery = searchFriendshipQuery.Where(p => request.FilterStatus.Contains(p.Status));
        }

        var totalCount = await searchFriendshipQuery.CountAsync(cancellationToken);

        var friendships = await searchFriendshipQuery.OrderByDescending(p => p.CreatedAt)
            .Skip(pagedRequest.SkipCount)
            .Take(pagedRequest.PageSize)
            .ToListAsync(cancellationToken);

        if (friendships == null)
        {
            return PagedResultDto<FriendshipDto>.Failure("Unexpected error occured!")
                .WithPage(pagedRequest.PageIndex, totalCount);
        }

        var userIds = friendships.Select(p => p.UserId == request.UserId ? p.FriendId : p.UserId);
        var users = await _userRepository.GetAll().Where(u => userIds.Contains(u.Id)).ToListAsync();

        var result = friendships.Select(_mapper.Map<FriendshipDto>).ToList();
        result.ForEach(fs =>
        {
            var userInfo = users.FirstOrDefault(u => u.Id == fs.UserId || u.Id == fs.FriendId);
            fs.User = _mapper.Map<UserDto>(userInfo);
        });

        return PagedResultDto<FriendshipDto>.Success(result)
            .WithPage(pagedRequest.PageIndex, totalCount);
    }
}
