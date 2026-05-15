using System;
using GoingMy.Shared.Events;
using MassTransit;

namespace GoingMy.Search.API.Consumers;

public class UserUpdatedEventConsumer : IConsumer<UserRegisteredEvent>, IConsumer<UserUpdatedEvent>, IConsumer<UserDeletedEvent>
{
  public Task Consume(ConsumeContext<UserRegisteredEvent> context)
  {
    throw new NotImplementedException();
  }

  public Task Consume(ConsumeContext<UserUpdatedEvent> context)
  {
    throw new NotImplementedException();
  }

  public Task Consume(ConsumeContext<UserDeletedEvent> context)
  {
    throw new NotImplementedException();
  }
}
