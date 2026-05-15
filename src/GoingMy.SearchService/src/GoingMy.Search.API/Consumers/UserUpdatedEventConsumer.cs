using Elastic.Clients.Elasticsearch;
using GoingMy.Search.API.Enums;
using GoingMy.Search.API.Models;
using GoingMy.Shared.Events;
using MassTransit;

namespace GoingMy.Search.API.Consumers;

public class UserUpdatedEventConsumer : IConsumer<UserRegisteredEvent>, IConsumer<UserUpdatedEvent>, IConsumer<UserDeletedEvent>
{
  private const string IndexName = "users";
  private readonly ElasticsearchClient _esClient;

  public UserUpdatedEventConsumer(IElasticsearchClientSettings esSettings)
  {
    _esClient = new ElasticsearchClient(esSettings);
  }

  public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
  {
    var evt = context.Message;

    var userDoc = new UserDoc
    {
      Id = evt.UserId.ToString(),
      Username = evt.Username,
      FirstName = evt.FirstName,
      LastName = evt.LastName,
      CreatedAt = DateTime.UtcNow
    };

    await _esClient.IndexAsync(userDoc, idx => idx.Index(IndexName).Id(userDoc.Id), context.CancellationToken);
  }

  public Task Consume(ConsumeContext<UserUpdatedEvent> context)
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
      UpdatedAt = evt.UpdatedAt
    };

    return _esClient.IndexAsync(userDoc, idx => idx.Index(IndexName).Id(userDoc.Id), context.CancellationToken);
  }

  public Task Consume(ConsumeContext<UserDeletedEvent> context)
  {
    var evt = context.Message;
    return _esClient.DeleteAsync<UserDoc>(evt.UserId.ToString(), idx => idx.Index(IndexName), context.CancellationToken);
  }
}
