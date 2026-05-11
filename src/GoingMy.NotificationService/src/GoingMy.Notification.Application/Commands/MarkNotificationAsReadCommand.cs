using GoingMy.Notification.Domain.Repositories;
using MediatR;

namespace GoingMy.Notification.Application.Commands;

public record MarkNotificationAsReadCommand(string NotificationId, string UserId) : IRequest;

public class MarkNotificationAsReadCommandHandler(INotificationRepository repository)
    : IRequestHandler<MarkNotificationAsReadCommand>
{
    public async Task Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        await repository.MarkAsReadAsync(request.NotificationId, request.UserId, cancellationToken);
    }
}
