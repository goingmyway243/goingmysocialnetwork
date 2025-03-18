using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Contents.Commands;

public class UpdateContentCommandHandler : IRequestHandler<UpdateContentCommand, CommandResult<ContentDto>>
{
    private readonly IRepository<ContentEntity> _contentRepository;
    private readonly IMapper _mapper;

    public UpdateContentCommandHandler(IRepository<ContentEntity> contentRepository, IMapper mapper)
    {
        _contentRepository = contentRepository;
        _mapper = mapper;
    }
    public async Task<CommandResult<ContentDto>> Handle(UpdateContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(request.Id);
        if (content == null)
        {
            return CommandResult<ContentDto>.Failure("Content not found");
        }

        content.TextContent = request.TextContent;
        await _contentRepository.UpdateAsync(content);

        return CommandResult<ContentDto>.Success(_mapper.Map<ContentDto>(content));
    }
}
