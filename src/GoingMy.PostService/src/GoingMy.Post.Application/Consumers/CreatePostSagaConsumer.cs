using GoingMy.Post.Application.Commands;
using GoingMy.Post.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;
using MongoDB.Bson;

namespace GoingMy.Post.Application.Consumers;

public class CreatePostSagaConsumer(IPostRepository postRepository, IPublishEndpoint publishEndpoint)
    : IConsumer<CreatePostForSagaCommand>
{
    public async Task Consume(ConsumeContext<CreatePostForSagaCommand> context)
    {
        var msg = context.Message;
        try
        {
            var post = new Domain.Entities.Post(
                id: ObjectId.GenerateNewId().ToString(),
                content: msg.Content,
                userId: msg.UserId,
                username: msg.Username,
                createdAt: DateTime.UtcNow);

            await postRepository.AddAsync(post, context.CancellationToken);

            await publishEndpoint.Publish(new PostCreatedForSagaEvent
            {
                CorrelationId = msg.CorrelationId,
                PostId = post.Id
            }, context.CancellationToken);
        }
        catch (Exception ex)
        {
            await publishEndpoint.Publish(new PostCreationFailedEvent
            {
                CorrelationId = msg.CorrelationId,
                Reason = ex.Message
            }, context.CancellationToken);
        }
    }
}
