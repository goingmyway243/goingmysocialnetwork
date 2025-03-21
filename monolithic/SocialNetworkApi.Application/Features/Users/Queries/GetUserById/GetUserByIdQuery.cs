using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Users.Queries
{
    public class GetUserByIdQuery : IRequest<QueryResultDto<UserDto>>
    {
        public Guid Id { get; set; }
    }
}