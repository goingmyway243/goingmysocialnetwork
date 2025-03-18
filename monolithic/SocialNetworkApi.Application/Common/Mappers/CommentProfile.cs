using AutoMapper;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Features.Comments.Commands;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Application.Common.Mappers;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<CommentEntity, CommentDto>();
        CreateMap<CommentDto, CommentEntity>();
        CreateMap<CreateCommentCommand, CommentEntity>();
    }
}
