using AutoMapper;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Application.Common.Mappers;

public class FriendshipProfile : Profile
{
    public FriendshipProfile()
    {
        CreateMap<FriendshipEntity, FriendshipDto>();
        CreateMap<FriendshipDto, FriendshipEntity>();
    }
}
