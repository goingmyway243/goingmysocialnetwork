using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Contents.Commands;

public class DeleteContentCommandHandler : IRequestHandler<DeleteContentCommand, CommandResult<Guid>>
{
    private readonly IRepository<ContentEntity> _contentRepository;

    public DeleteContentCommandHandler(IRepository<ContentEntity> contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<CommandResult<Guid>> Handle(DeleteContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(request.Id);
        if (content == null)
        {
            return CommandResult<Guid>.Failure("Content not found");
        }

        await _contentRepository.DeleteAsync(content);

        return CommandResult<Guid>.Success(content.Id);
    }
}
