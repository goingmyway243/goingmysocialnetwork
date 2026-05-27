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

            post.Author = new Domain.Entities.User
            {
                Id = msg.UserId,
                UserName = msg.Username,
                FirstName = string.IsNullOrWhiteSpace(msg.FirstName) ? msg.Username : msg.FirstName,
                LastName = msg.LastName ?? string.Empty
            };

            var createdPost = await postRepository.AddAsync(post, context.CancellationToken);

            await publishEndpoint.Publish(new PostCreatedEvent
            {
                PostId = createdPost.Id,
                UserId = createdPost.UserId,
                Username = createdPost.Username,
                Content = createdPost.Content,
                MediaAttachments = createdPost.MediaAttachments?.Select(m => m.Url).ToList() ?? [],
                CreatedAt = createdPost.CreatedAt
            }, context.CancellationToken);

            await publishEndpoint.Publish(new PostCreatedForSagaEvent
            {
                CorrelationId = msg.CorrelationId,
                PostId = createdPost.Id
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
