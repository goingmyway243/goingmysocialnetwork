using Elastic.Clients.Elasticsearch;
using GoingMy.Search.API.Infrastructure;
using GoingMy.Search.API.Models;
using GoingMy.Shared.Events;
using MassTransit;

namespace GoingMy.Search.API.Consumers;

public class PostInteractionConsumer(ElasticsearchClient esClient)
    : IConsumer<PostLikedEvent>,
      IConsumer<CommentAddedEvent>
{
    public async Task Consume(ConsumeContext<PostLikedEvent> context)
    {
        var evt = context.Message;

        await esClient.UpdateAsync<PostDoc, object>(
            ElasticsearchIndexMappings.PostsIndex,
            evt.PostId,
            u => u.Script(s => s
                .Source("ctx._source.likes = (ctx._source.likes != null ? ctx._source.likes : 0) + 1")),
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<CommentAddedEvent> context)
    {
        var evt = context.Message;

        await esClient.UpdateAsync<PostDoc, object>(
            ElasticsearchIndexMappings.PostsIndex,
            evt.PostId,
            u => u.Script(s => s
                .Source("ctx._source.comments = (ctx._source.comments != null ? ctx._source.comments : 0) + 1")),
            context.CancellationToken);
    }
}
