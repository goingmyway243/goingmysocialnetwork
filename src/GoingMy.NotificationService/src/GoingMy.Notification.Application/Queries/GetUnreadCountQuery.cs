using GoingMy.Notification.Domain.Repositories;
using MediatR;

namespace GoingMy.Notification.Application.Queries;

public record GetUnreadCountQuery(string UserId) : IRequest<long>;

public class GetUnreadCountQueryHandler(INotificationRepository repository)
    : IRequestHandler<GetUnreadCountQuery, long>
{
    public async Task<long> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetUnreadCountAsync(request.UserId, cancellationToken);
    }
}
