using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Queries;

/// <summary>
/// Returns true if <see cref="BlockerId"/> has blocked <see cref="BlockeeId"/>.
/// </summary>
public record CheckBlockStatusQuery(Guid BlockerId, Guid BlockeeId) : IRequest<bool>;

public class CheckBlockStatusQueryHandler(IUserBlockRepository userBlockRepository)
    : IRequestHandler<CheckBlockStatusQuery, bool>
{
    public async Task<bool> Handle(CheckBlockStatusQuery request, CancellationToken cancellationToken)
    {
        return await userBlockRepository.ExistsAsync(
            request.BlockerId, request.BlockeeId, cancellationToken);
    }
}
