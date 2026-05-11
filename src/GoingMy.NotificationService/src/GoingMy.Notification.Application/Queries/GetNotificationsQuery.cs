using GoingMy.Notification.Application.Commands;
using GoingMy.Notification.Application.Dtos;
using GoingMy.Notification.Domain.Repositories;
using MediatR;

namespace GoingMy.Notification.Application.Queries;

public record GetNotificationsQuery(string UserId, int PageNumber, int PageSize) : IRequest<NotificationPagedResult>;

public class GetNotificationsQueryHandler(INotificationRepository repository)
    : IRequestHandler<GetNotificationsQuery, NotificationPagedResult>
{
    public async Task<NotificationPagedResult> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var (items, hasMore) = await repository.GetByRecipientPagedAsync(
            request.UserId, request.PageNumber, request.PageSize, cancellationToken);

        var dtos = items.Select(CreateNotificationCommandHandler.MapToDto);
        return new NotificationPagedResult(dtos, hasMore, request.PageNumber, request.PageSize);
    }
}
