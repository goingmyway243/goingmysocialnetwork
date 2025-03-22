using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Users.Queries;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, PagedResultDto<UserDto>>
{
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IRepository<FriendshipEntity> _friendshipRepository;
    private readonly IMapper _mapper;

    public SearchUsersQueryHandler(
        IRepository<UserEntity> userRepository,
        IRepository<FriendshipEntity> friendshipRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<UserDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = request.PagedRequest;
        var searchUserQuery = _userRepository.GetAll();

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            searchUserQuery = searchUserQuery.Where(p => p.FullName!.Contains(request.SearchText));
        }

        var totalCount = await searchUserQuery.CountAsync(cancellationToken);

        searchUserQuery = searchUserQuery.Skip(pagedRequest.SkipCount)
        .Take(pagedRequest.PageSize)
        .OrderByDescending(p => p.ModifiedAt ?? p.CreatedAt);

        var result = await searchUserQuery.ToListAsync(cancellationToken);
        if (result == null)
        {
            return PagedResultDto<UserDto>.Failure("Unexpected error occured!")
                .WithPage(pagedRequest.PageIndex, totalCount);
        }

        var items = result.Select(_mapper.Map<UserDto>).ToList();
        if (request.IncludeFriendship)
        {
            var itemIds = items.Select(u => u.Id);
            var friendships = await _friendshipRepository.GetAll()
                .Where(fs => (fs.UserId == request.RequestUserId && itemIds.Contains(fs.FriendId))
                             || (fs.FriendId == request.RequestUserId && itemIds.Contains(fs.UserId)))
                .ToListAsync();

            items.ForEach(item =>
            {
                if (item.Id != request.RequestUserId)
                {
                    var friendship = friendships.FirstOrDefault(p => p.UserId == item.Id || p.FriendId == item.Id);
                    item.Friendship = _mapper.Map<FriendshipDto>(friendship);
                }
            });
        }

        return PagedResultDto<UserDto>.Success(items)
            .WithPage(pagedRequest.PageIndex, totalCount);
    }
}
