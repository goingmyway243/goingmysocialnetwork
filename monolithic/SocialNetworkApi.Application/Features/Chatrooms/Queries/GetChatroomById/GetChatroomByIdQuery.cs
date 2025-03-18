using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Chatrooms.Queries.GetChatroomById;

public class GetChatroomByIdQuery : IRequest<QueryResult<ChatroomDto>>
{
    public Guid Id { get; set; }
}
