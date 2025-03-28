using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Users.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, QueryResultDto<UserDto>>
{
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IRepository<UserEntity> userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<QueryResultDto<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);
        if (user == null)
        {
            return QueryResultDto<UserDto>.Failure("User not found.");
        }

        var result = _mapper.Map<UserDto>(user);
        return QueryResultDto<UserDto>.Success(result);
    }
}
