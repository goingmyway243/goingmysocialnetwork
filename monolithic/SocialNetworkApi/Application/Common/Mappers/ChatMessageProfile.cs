using AutoMapper;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Features.ChatMessages.Commands;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Application.Common.Mappers;

public class ChatMessageProfile : Profile
{
    public ChatMessageProfile()
    {
        CreateMap<ChatMessageEntity, ChatMessageDto>();
        CreateMap<ChatMessageDto, ChatMessageEntity>();
        CreateMap<CreateChatMessageCommand, ChatMessageEntity>();
    }
}
