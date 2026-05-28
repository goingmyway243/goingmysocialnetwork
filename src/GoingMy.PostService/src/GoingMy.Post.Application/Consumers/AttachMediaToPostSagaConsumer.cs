using GoingMy.Post.Domain.Entities;
using GoingMy.Post.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;

namespace GoingMy.Post.Application.Consumers;

public class AttachMediaToPostSagaConsumer(IPostRepository postRepository, IPublishEndpoint publishEndpoint)
    : IConsumer<AttachMediaToPostCommand>
{
    public async Task Consume(ConsumeContext<AttachMediaToPostCommand> context)
    {
        var msg = context.Message;

        var post = await postRepository.GetByIdAsync(msg.PostId, context.CancellationToken)
            ?? throw new InvalidOperationException($"Post '{msg.PostId}' not found for media attachment.");

        var attachments = msg.MediaFiles.Select(f => new MediaAttachment
        {
            FileId = f.FileId,
            Url = f.Url,
            ContentType = f.ContentType
        });

        post.AttachMedia(attachments);
        await postRepository.UpdateAsync(post, context.CancellationToken);

        await publishEndpoint.Publish(new PostUpdatedEvent
        {
            PostId = post.Id,
            UserId = post.UserId,
            Content = post.Content,
            MediaAttachments = post.MediaAttachments.Select(m => new MediaAttachmentInfo(m.FileId, m.Url, m.ContentType, m.Width, m.Height)).ToList(),
            UpdatedAt = post.UpdatedAt ?? DateTime.UtcNow
        }, context.CancellationToken);

        await publishEndpoint.Publish(new MediaAttachedToPostEvent
        {
            CorrelationId = msg.CorrelationId,
            PostId = post.Id
        }, context.CancellationToken);
    }
}
