using AutoMapper;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Application.Common.Mappers;

public class ContentProfile : Profile
{
    public ContentProfile()
    {
        CreateMap<ContentEntity, ContentDto>();
        CreateMap<ContentDto, ContentEntity>();
    }
}
