using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Commands;

public record UnblockUserCommand(Guid BlockerId, Guid BlockeeId) : IRequest;

public class UnblockUserCommandHandler(IUserBlockRepository userBlockRepository)
    : IRequestHandler<UnblockUserCommand>
{
    public async Task Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var isBlocked = await userBlockRepository.ExistsAsync(
            request.BlockerId, request.BlockeeId, cancellationToken);

        if (!isBlocked)
            throw new InvalidOperationException("User is not blocked.");

        await userBlockRepository.DeleteAsync(request.BlockerId, request.BlockeeId, cancellationToken);
    }
}
