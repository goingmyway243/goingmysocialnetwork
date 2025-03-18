using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.ChatMessages.Queries;

public class GetChatMessageByIdQueryHandler : IRequestHandler<GetChatMessageByIdQuery, QueryResult<ChatMessageDto>>
{
    private readonly IRepository<ChatMessageEntity> _chatMessageRepository;
    private readonly IMapper _mapper;

    public GetChatMessageByIdQueryHandler(IRepository<ChatMessageEntity> chatMessageRepository, IMapper mapper)
    {
        _chatMessageRepository = chatMessageRepository;
        _mapper = mapper;
    }

    public Task<QueryResult<ChatMessageDto>> Handle(GetChatMessageByIdQuery request, CancellationToken cancellationToken)
    {
        var chatMessage = _chatMessageRepository.GetByIdAsync(request.Id);
        if (chatMessage == null)
        {
            return Task.FromResult(QueryResult<ChatMessageDto>.Failure("Chat message not found."));
        }

        return Task.FromResult(QueryResult<ChatMessageDto>.Success(_mapper.Map<ChatMessageDto>(chatMessage)));
    }
}
