using Elastic.Clients.Elasticsearch;
using GoingMy.Search.API.Enums;
using GoingMy.Search.API.Infrastructure;
using GoingMy.Search.API.Models;
using GoingMy.Shared.Events;
using MassTransit;

namespace GoingMy.Search.API.Consumers;

public class UserEventConsumer(ElasticsearchClient esClient)
    : IConsumer<UserRegisteredEvent>,
      IConsumer<UserCreatedEvent>,
      IConsumer<UserUpdatedEvent>,
      IConsumer<UserDeletedEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var evt = context.Message;
        var userDoc = new UserDoc
        {
            Id = evt.UserId.ToString(),
            Username = evt.Username,
            FirstName = evt.FirstName,
            LastName = evt.LastName,
            IsActive = true,
            IsVerified = evt.IsVerified,
            CreatedAt = evt.RegisteredAt,
            Suggest = BuildSuggest(evt.Username, evt.FirstName, evt.LastName)
        };

        await esClient.IndexAsync(userDoc,
            idx => idx.Index(ElasticsearchIndexMappings.UsersIndex).Id(userDoc.Id),
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var evt = context.Message;
        var userDoc = new UserDoc
        {
            Id = evt.UserId.ToString(),
            Username = evt.Username,
            FirstName = evt.FirstName,
            LastName = evt.LastName,
            Gender = (Gender)evt.Gender,
            IsVerified = evt.IsVerified,
            IsActive = evt.IsActive,
            CreatedAt = evt.CreatedAt,
            Suggest = BuildSuggest(evt.Username, evt.FirstName, evt.LastName)
        };

        await esClient.IndexAsync(userDoc,
            idx => idx.Index(ElasticsearchIndexMappings.UsersIndex).Id(userDoc.Id),
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var evt = context.Message;
        var userDoc = new UserDoc
        {
            Id = evt.UserId.ToString(),
            Username = evt.Username,
            FirstName = evt.FirstName,
            LastName = evt.LastName,
            Bio = evt.Bio,
            AvatarUrl = evt.AvatarUrl,
            CoverUrl = evt.CoverUrl,
            DateOfBirth = evt.DateOfBirth,
            Gender = (Gender)evt.Gender,
            Location = evt.Location,
            WebsiteUrl = evt.WebsiteUrl,
            FollowersCount = evt.FollowersCount,
            FollowingCount = evt.FollowingCount,
            PostsCount = evt.PostsCount,
            IsVerified = evt.IsVerified,
            IsPrivate = evt.IsPrivate,
            IsActive = evt.IsActive,
            Interests = evt.Interests,
            UpdatedAt = evt.UpdatedAt,
            Suggest = BuildSuggest(evt.Username, evt.FirstName, evt.LastName)
        };

        await esClient.IndexAsync(userDoc,
            idx => idx.Index(ElasticsearchIndexMappings.UsersIndex).Id(userDoc.Id),
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var evt = context.Message;
        await esClient.DeleteAsync<UserDoc>(evt.UserId.ToString(),
            idx => idx.Index(ElasticsearchIndexMappings.UsersIndex),
            context.CancellationToken);
    }

    private static SuggestField BuildSuggest(string username, string firstName, string lastName) =>
        new() { Input = [username, $"{firstName} {lastName}", firstName, lastName] };
}
