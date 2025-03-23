using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.ChatMessages.Queries;

public class SearchChatMessagesQuery : IRequest<PagedResultDto<ChatMessageDto>>
{
    public Guid ChatroomId { get; set; }
    public string SearchText { get; set; } = string.Empty;
    public PagedRequestDto PagedRequest { get; set; } = new PagedRequestDto();
}
