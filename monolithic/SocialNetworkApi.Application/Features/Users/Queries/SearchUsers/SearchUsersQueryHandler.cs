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
    private readonly IMapper _mapper;

    public SearchUsersQueryHandler(IRepository<UserEntity> userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<UserDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = request.PagedRequest;
        var query = _userRepository.GetAll();

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            query = query.Where(p => p.FullName!.Contains(request.SearchText));
        }

        var totalCount = await query.CountAsync();

        query = query.Skip(pagedRequest.PageIndex * pagedRequest.PageSize)
        .Take(pagedRequest.PageSize)
        .OrderByDescending(p => p.ModifiedAt ?? p.CreatedAt);

        var result = await query.ToListAsync();
        if (result == null)
        {
            return PagedResultDto<UserDto>.Failure("Unexpected error occured!")
                .WithPage(pagedRequest.PageIndex, totalCount);
        }

        var items = result.Select(_mapper.Map<UserDto>);
        return PagedResultDto<UserDto>.Success(items)
            .WithPage(pagedRequest.PageIndex, totalCount);
    }
}
