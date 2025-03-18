using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Users.Queries
{
    public class GetUserByIdQuery : IRequest<QueryResult<UserDto>>
    {
        public Guid Id { get; set; }
    }
}