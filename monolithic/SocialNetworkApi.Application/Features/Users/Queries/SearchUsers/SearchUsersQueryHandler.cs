using AutoMapper;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
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
        var builder = Builders<UserEntity>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            filter &= builder.Regex(p => p.FullName, new BsonRegularExpression(request.SearchText, "i"));
        }

        var totalCount = await _userRepository.GetAll().CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var users = await _userRepository.GetAll()
                    .Aggregate()
                    .Match(filter)
                    .Project(p => new {
                        Document = p,
                        SortDate = p.ModifiedAt ?? p.CreatedAt
                    })
                    .SortByDescending(p => p.SortDate)
                    .Project(p => p.Document)
                    .Skip(pagedRequest.SkipCount)
                    .Limit(pagedRequest.PageSize)
                    .ToListAsync(cancellationToken);

        var result = users.Select(_mapper.Map<UserDto>).ToList();
        if (request.IncludeFriendship)
        {
            await IncludeFriendship(request, result);
        }

        return PagedResultDto<UserDto>.Success(result)
            .WithPage(pagedRequest.PageIndex, (int)totalCount);
    }

    private async Task IncludeFriendship(SearchUsersQuery request, List<UserDto> result)
    {
        var itemIds = result.Select(u => u.Id);
        var friendships = await _friendshipRepository.FindAsync(fs => (fs.UserId == request.RequestUserId && itemIds.Contains(fs.FriendId))
                         || (fs.FriendId == request.RequestUserId && itemIds.Contains(fs.UserId)));

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
