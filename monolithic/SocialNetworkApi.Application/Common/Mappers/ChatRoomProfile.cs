using AutoMapper;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Application.Common.Mappers;

public class ChatRoomProfile : Profile
{
    public ChatRoomProfile()
    {
        CreateMap<ChatroomEntity, ChatroomEntity>();
        CreateMap<ChatroomDto, ChatroomEntity>();
    }
}
