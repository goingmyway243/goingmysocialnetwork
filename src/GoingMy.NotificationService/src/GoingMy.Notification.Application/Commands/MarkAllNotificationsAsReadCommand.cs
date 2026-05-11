using GoingMy.Notification.Domain.Repositories;
using MediatR;

namespace GoingMy.Notification.Application.Commands;

public record MarkAllNotificationsAsReadCommand(string UserId) : IRequest;

public class MarkAllNotificationsAsReadCommandHandler(INotificationRepository repository)
    : IRequestHandler<MarkAllNotificationsAsReadCommand>
{
    public async Task Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        await repository.MarkAllAsReadAsync(request.UserId, cancellationToken);
    }
}
