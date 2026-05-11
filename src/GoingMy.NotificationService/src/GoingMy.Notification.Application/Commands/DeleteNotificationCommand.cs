using GoingMy.Notification.Domain.Repositories;
using MediatR;

namespace GoingMy.Notification.Application.Commands;

public record DeleteNotificationCommand(string NotificationId, string UserId) : IRequest;

public class DeleteNotificationCommandHandler(INotificationRepository repository)
    : IRequestHandler<DeleteNotificationCommand>
{
    public async Task Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(request.NotificationId, request.UserId, cancellationToken);
    }
}
