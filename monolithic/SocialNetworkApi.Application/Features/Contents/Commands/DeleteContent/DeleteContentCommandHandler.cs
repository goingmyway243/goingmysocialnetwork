using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Contents.Commands;

public class DeleteContentCommandHandler : IRequestHandler<DeleteContentCommand, CommandResultDto<Guid>>
{
    private readonly IRepository<ContentEntity> _contentRepository;

    public DeleteContentCommandHandler(IRepository<ContentEntity> contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<CommandResultDto<Guid>> Handle(DeleteContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(request.Id);
        if (content == null)
        {
            return CommandResultDto<Guid>.Failure("Content not found");
        }

        await _contentRepository.DeleteAsync(content);

        return CommandResultDto<Guid>.Success(content.Id);
    }
}
