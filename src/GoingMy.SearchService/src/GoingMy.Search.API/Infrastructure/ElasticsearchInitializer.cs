using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using GoingMy.Search.API.Models;

namespace GoingMy.Search.API.Infrastructure;

public class ElasticsearchInitializer(ElasticsearchClient esClient, ILogger<ElasticsearchInitializer> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await EnsureUsersIndexAsync(cancellationToken);
        await EnsurePostsIndexAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsureUsersIndexAsync(CancellationToken ct)
    {
        var existsResponse = await esClient.Indices.ExistsAsync(ElasticsearchIndexMappings.UsersIndex, ct);
        if (existsResponse.Exists)
        {
            logger.LogInformation("Elasticsearch index '{Index}' already exists", ElasticsearchIndexMappings.UsersIndex);
            return;
        }

        var createResponse = await esClient.Indices.CreateAsync(ElasticsearchIndexMappings.UsersIndex, c => c
            .Mappings(m => m
                .Properties(new Properties
                {
                    ["id"] = new KeywordProperty(),
                    ["username"] = new TextProperty { Analyzer = "standard" },
                    ["firstName"] = new TextProperty { Analyzer = "standard" },
                    ["lastName"] = new TextProperty { Analyzer = "standard" },
                    ["bio"] = new TextProperty { Analyzer = "standard" },
                    ["location"] = new KeywordProperty(),
                    ["avatarUrl"] = new KeywordProperty(),
                    ["coverUrl"] = new KeywordProperty(),
                    ["websiteUrl"] = new KeywordProperty(),
                    ["isVerified"] = new BooleanProperty(),
                    ["isPrivate"] = new BooleanProperty(),
                    ["isActive"] = new BooleanProperty(),
                    ["followersCount"] = new IntegerNumberProperty(),
                    ["followingCount"] = new IntegerNumberProperty(),
                    ["postsCount"] = new IntegerNumberProperty(),
                    ["createdAt"] = new DateProperty(),
                    ["updatedAt"] = new DateProperty(),
                    ["suggest"] = new CompletionProperty()
                })
            ), ct);

        LogCreateResult(createResponse.IsValidResponse, ElasticsearchIndexMappings.UsersIndex,
            createResponse.ElasticsearchServerError?.Error?.Reason);
    }

    private async Task EnsurePostsIndexAsync(CancellationToken ct)
    {
        var existsResponse = await esClient.Indices.ExistsAsync(ElasticsearchIndexMappings.PostsIndex, ct);
        if (existsResponse.Exists)
        {
            logger.LogInformation("Elasticsearch index '{Index}' already exists", ElasticsearchIndexMappings.PostsIndex);
            return;
        }

        var createResponse = await esClient.Indices.CreateAsync(ElasticsearchIndexMappings.PostsIndex, c => c
            .Mappings(m => m
                .Properties(new Properties
                {
                    ["id"] = new KeywordProperty(),
                    ["userId"] = new KeywordProperty(),
                    ["username"] = new KeywordProperty(),
                    ["content"] = new TextProperty { Analyzer = "standard" },
                    ["likes"] = new IntegerNumberProperty(),
                    ["comments"] = new IntegerNumberProperty(),
                    ["createdAt"] = new DateProperty(),
                    ["updatedAt"] = new DateProperty(),
                    ["suggest"] = new CompletionProperty(),
                    ["mediaAttachments"] = new ObjectProperty
                    {
                        Properties = new Properties
                        {
                            ["fileId"] = new KeywordProperty(),
                            ["url"] = new KeywordProperty(),
                            ["contentType"] = new KeywordProperty(),
                            ["width"] = new IntegerNumberProperty(),
                            ["height"] = new IntegerNumberProperty()
                        }
                    }
                })
            ), ct);

        LogCreateResult(createResponse.IsValidResponse, ElasticsearchIndexMappings.PostsIndex,
            createResponse.ElasticsearchServerError?.Error?.Reason);
    }

    private void LogCreateResult(bool success, string indexName, string? errorReason)
    {
        if (success)
            logger.LogInformation("Created Elasticsearch index '{Index}'", indexName);
        else
            logger.LogError("Failed to create Elasticsearch index '{Index}': {Error}", indexName, errorReason);
    }
}
