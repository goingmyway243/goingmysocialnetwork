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

        var users = await searchUserQuery.OrderByDescending(p => p.ModifiedAt ?? p.CreatedAt)
            .Skip(pagedRequest.SkipCount)
            .Take(pagedRequest.PageSize)
            .ToListAsync(cancellationToken);

        var result = users.Select(_mapper.Map<UserDto>).ToList();
        if (request.IncludeFriendship)
        {
            await IncludeFriendship(request, result);
        }

        return PagedResultDto<UserDto>.Success(result)
            .WithPage(pagedRequest.PageIndex, totalCount);
    }

    private async Task IncludeFriendship(SearchUsersQuery request, List<UserDto> result)
    {
        var itemIds = result.Select(u => u.Id);
        var friendships = await _friendshipRepository.GetAll()
            .Where(fs => (fs.UserId == request.RequestUserId && itemIds.Contains(fs.FriendId))
                         || (fs.FriendId == request.RequestUserId && itemIds.Contains(fs.UserId)))
            .ToListAsync();

        result.ForEach(item =>
        {
            if (item.Id != request.RequestUserId)
            {
                var friendship = friendships.FirstOrDefault(p => p.UserId == item.Id || p.FriendId == item.Id);
                item.Friendship = _mapper.Map<FriendshipDto>(friendship);
            }
        });
    }
}
