using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.ChatMessages.Queries;

public class SearchChatMessagesQuery : IRequest<PagedResultDto<ChatMessageDto>>
{
    public Guid ChatroomId { get; set; }
    public PagedRequestDto PagedRequest { get; set; } = new PagedRequestDto();
}
