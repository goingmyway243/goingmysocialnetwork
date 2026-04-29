using GoingMy.User.Domain.Entities;
using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Commands;

public record BlockUserCommand(Guid BlockerId, Guid BlockeeId) : IRequest;

public class BlockUserCommandHandler(
    IUserBlockRepository userBlockRepository,
    IUserProfileRepository userProfileRepository)
    : IRequestHandler<BlockUserCommand>
{
    public async Task Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        if (request.BlockerId == request.BlockeeId)
            throw new InvalidOperationException("A user cannot block themselves.");

        var alreadyBlocked = await userBlockRepository.ExistsAsync(
            request.BlockerId, request.BlockeeId, cancellationToken);

        if (alreadyBlocked)
            throw new InvalidOperationException("User is already blocked.");

        var blockerExists = await userProfileRepository.GetByIdAsync(request.BlockerId, cancellationToken);
        if (blockerExists is null)
            throw new KeyNotFoundException($"User profile '{request.BlockerId}' not found.");

        var blockeeExists = await userProfileRepository.GetByIdAsync(request.BlockeeId, cancellationToken);
        if (blockeeExists is null)
            throw new KeyNotFoundException($"User profile '{request.BlockeeId}' not found.");

        var block = new UserBlock(request.BlockerId, request.BlockeeId);
        await userBlockRepository.CreateAsync(block, cancellationToken);
    }
}
