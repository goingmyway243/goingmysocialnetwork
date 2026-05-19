using Elastic.Clients.Elasticsearch;
using GoingMy.Search.API.Infrastructure;
using GoingMy.Search.API.Models;
using GoingMy.Shared.Events;
using MassTransit;

namespace GoingMy.Search.API.Consumers;

public class PostEventConsumer(ElasticsearchClient esClient)
    : IConsumer<PostCreatedEvent>,
      IConsumer<PostUpdatedEvent>,
      IConsumer<PostDeletedEvent>
{
    public async Task Consume(ConsumeContext<PostCreatedEvent> context)
    {
        var evt = context.Message;
        var postDoc = new PostDoc
        {
            Id = evt.PostId,
            UserId = evt.UserId,
            Username = evt.Username,
            Content = evt.Content,
            MediaAttachments = evt.MediaAttachments,
            CreatedAt = evt.CreatedAt,
            Suggest = BuildSuggest(evt.Content)
        };

        await esClient.IndexAsync(postDoc,
            idx => idx.Index(ElasticsearchIndexMappings.PostsIndex).Id(postDoc.Id),
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<PostUpdatedEvent> context)
    {
        var evt = context.Message;

        await esClient.UpdateAsync<PostDoc, object>(
            ElasticsearchIndexMappings.PostsIndex,
            evt.PostId,
            u => u.Doc(new
            {
                content = evt.Content,
                mediaAttachments = evt.MediaAttachments,
                updatedAt = evt.UpdatedAt,
                suggest = BuildSuggest(evt.Content)
            }),
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<PostDeletedEvent> context)
    {
        var evt = context.Message;
        await esClient.DeleteAsync<PostDoc>(evt.PostId,
            idx => idx.Index(ElasticsearchIndexMappings.PostsIndex),
            context.CancellationToken);
    }

    private static SuggestField BuildSuggest(string content)
    {
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var inputs = words.Length > 0
            ? words.Take(5).Prepend(content[..Math.Min(50, content.Length)]).Distinct().ToArray()
            : [content];
        return new SuggestField { Input = inputs };
    }
}
