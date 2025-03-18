using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.ChatMessages.Queries;

public class GetChatMessageByIdQuery : IRequest<QueryResult<ChatMessageDto>>
{
    public Guid Id { get; set; }
}
