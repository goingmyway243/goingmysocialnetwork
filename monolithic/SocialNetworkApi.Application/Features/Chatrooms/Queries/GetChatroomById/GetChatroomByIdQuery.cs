using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Chatrooms.Queries;

public class GetChatroomByIdQuery : IRequest<QueryResultDto<ChatroomDto>>
{
    public Guid Id { get; set; }
}
