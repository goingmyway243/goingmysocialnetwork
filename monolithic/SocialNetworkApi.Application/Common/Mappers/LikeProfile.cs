using AutoMapper;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Features.Likes.Commands;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Application.Common.Mappers;

public class LikeProfile : Profile
{
    public LikeProfile()
    {
        CreateMap<LikeEntity, LikeDto>();
        CreateMap<LikeDto, LikeEntity>();
        CreateMap<ToggleLikeCommand, LikeEntity>();
    }
}
