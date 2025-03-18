using AutoMapper;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Features.Posts.Commands;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Application.Common.Mappers;

public class PostProfile : Profile
{
    public PostProfile()
    {
        CreateMap<PostEntity, PostDto>();
        CreateMap<PostDto, PostEntity>();
        CreateMap<CreatePostCommand, PostEntity>();
    }
}
