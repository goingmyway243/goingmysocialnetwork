using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.ChatMessages.Queries;

public class GetChatMessageByIdQueryHandler : IRequestHandler<GetChatMessageByIdQuery, QueryResultDto<ChatMessageDto>>
{
    private readonly IRepository<ChatMessageEntity> _chatMessageRepository;
    private readonly IMapper _mapper;

    public GetChatMessageByIdQueryHandler(IRepository<ChatMessageEntity> chatMessageRepository, IMapper mapper)
    {
        _chatMessageRepository = chatMessageRepository;
        _mapper = mapper;
    }

    public Task<QueryResultDto<ChatMessageDto>> Handle(GetChatMessageByIdQuery request, CancellationToken cancellationToken)
    {
        var chatMessage = _chatMessageRepository.GetByIdAsync(request.Id);
        if (chatMessage == null)
        {
            return Task.FromResult(QueryResultDto<ChatMessageDto>.Failure("Chat message not found."));
        }

        return Task.FromResult(QueryResultDto<ChatMessageDto>.Success(_mapper.Map<ChatMessageDto>(chatMessage)));
    }
}
