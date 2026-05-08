using GoingMy.Upload.Domain.Enums;
using GoingMy.Upload.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;

namespace GoingMy.Upload.Application.Consumers;

public class ValidateMediaConsumer(IMediaFileRepository repository, IPublishEndpoint publishEndpoint)
    : IConsumer<ValidateMediaForSagaCommand>
{
    public async Task Consume(ConsumeContext<ValidateMediaForSagaCommand> context)
    {
        var msg = context.Message;

        var files = await repository.GetByIdsAsync(msg.MediaFileIds, context.CancellationToken);

        if (files.Count != msg.MediaFileIds.Count)
        {
            var found = files.Select(f => f.Id).ToHashSet();
            var missing = msg.MediaFileIds.Where(id => !found.Contains(id));
            await publishEndpoint.Publish(new MediaValidationFailedEvent
            {
                CorrelationId = msg.CorrelationId,
                Reason = $"Media files not found: {string.Join(", ", missing)}"
            }, context.CancellationToken);
            return;
        }

        var notOwnedByUser = files.Where(f => f.UploadedByUserId != msg.UserId).ToList();
        if (notOwnedByUser.Count != 0)
        {
            await publishEndpoint.Publish(new MediaValidationFailedEvent
            {
                CorrelationId = msg.CorrelationId,
                Reason = "One or more media files do not belong to the requesting user."
            }, context.CancellationToken);
            return;
        }

        var notReady = files.Where(f => f.Status != UploadStatus.Ready).ToList();
        if (notReady.Count != 0)
        {
            await publishEndpoint.Publish(new MediaValidationFailedEvent
            {
                CorrelationId = msg.CorrelationId,
                Reason = $"Media files are not in Ready state: {string.Join(", ", notReady.Select(f => f.Id))}"
            }, context.CancellationToken);
            return;
        }

        await publishEndpoint.Publish(new MediaValidatedEvent
        {
            CorrelationId = msg.CorrelationId,
            MediaFileIds = msg.MediaFileIds,
            MediaFiles = files.Select(f => new MediaFileInfo(f.Id, f.Url, f.ContentType)).ToList()
        }, context.CancellationToken);
    }
}
