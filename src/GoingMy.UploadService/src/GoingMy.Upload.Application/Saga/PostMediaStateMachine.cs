using GoingMy.Shared.Events;
using MassTransit;

namespace GoingMy.Upload.Application.Saga;

public class PostMediaStateMachine : MassTransitStateMachine<PostMediaSagaState>
{
    public State Validating { get; private set; } = null!;
    public State CreatingPost { get; private set; } = null!;
    public State AttachingMedia { get; private set; } = null!;

    public Event<PostWithMediaRequestedEvent> PostWithMediaRequested { get; private set; } = null!;
    public Event<MediaValidatedEvent> MediaValidated { get; private set; } = null!;
    public Event<MediaValidationFailedEvent> MediaValidationFailed { get; private set; } = null!;
    public Event<PostCreatedForSagaEvent> PostCreated { get; private set; } = null!;
    public Event<PostCreationFailedEvent> PostCreationFailed { get; private set; } = null!;
    public Event<MediaAttachedToPostEvent> MediaAttachedToPost { get; private set; } = null!;

    public PostMediaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => PostWithMediaRequested, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => MediaValidated, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => MediaValidationFailed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PostCreated, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PostCreationFailed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => MediaAttachedToPost, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Initially(
            When(PostWithMediaRequested)
                .Then(ctx =>
                {
                    ctx.Saga.UserId = ctx.Message.UserId;
                    ctx.Saga.Username = ctx.Message.Username;
                    ctx.Saga.FirstName = ctx.Message.FirstName;
                    ctx.Saga.LastName = ctx.Message.LastName;
                    ctx.Saga.Content = ctx.Message.Content;
                    ctx.Saga.MediaFileIds = [.. ctx.Message.MediaFileIds];
                    ctx.Saga.CreatedAt = DateTime.UtcNow;
                })
                .PublishAsync(ctx => ctx.Init<ValidateMediaForSagaCommand>(new ValidateMediaForSagaCommand
                {
                    CorrelationId = ctx.Message.CorrelationId,
                    UserId = ctx.Message.UserId,
                    MediaFileIds = [.. ctx.Message.MediaFileIds]
                }))
                .TransitionTo(Validating));

        During(Validating,
            When(MediaValidated)
                .Then(ctx =>
                {
                    ctx.Saga.ValidatedMediaFiles = [.. ctx.Message.MediaFiles];
                })
                .PublishAsync(ctx => ctx.Init<CreatePostForSagaCommand>(new CreatePostForSagaCommand
                {
                    CorrelationId = ctx.Message.CorrelationId,
                    UserId = ctx.Saga.UserId,
                    Username = ctx.Saga.Username,
                    FirstName = ctx.Saga.FirstName,
                    LastName = ctx.Saga.LastName,
                    Content = ctx.Saga.Content
                }))
                .TransitionTo(CreatingPost),

            When(MediaValidationFailed)
                .Then(ctx => ctx.Saga.ErrorMessage = ctx.Message.Reason)
                .ThenAsync(ctx => PublishOrphanedFileEventsAsync(ctx))
                .PublishAsync(ctx => ctx.Init<PostWithMediaSagaCompletedEvent>(new PostWithMediaSagaCompletedEvent
                {
                    CorrelationId = ctx.Message.CorrelationId,
                    UserId = ctx.Saga.UserId,
                    PostId = null,
                    IsSuccess = false,
                    ErrorMessage = ctx.Message.Reason
                }))
                .Finalize());

        During(CreatingPost,
            When(PostCreated)
                .Then(ctx => ctx.Saga.PostId = ctx.Message.PostId)
                .PublishAsync(ctx => ctx.Init<AttachMediaToPostCommand>(new AttachMediaToPostCommand
                {
                    CorrelationId = ctx.Message.CorrelationId,
                    PostId = ctx.Saga.PostId!,
                    MediaFiles = [.. ctx.Saga.ValidatedMediaFiles]
                }))
                .TransitionTo(AttachingMedia),

            When(PostCreationFailed)
                .Then(ctx => ctx.Saga.ErrorMessage = ctx.Message.Reason)
                .ThenAsync(async ctx =>
                {
                    if (!string.IsNullOrWhiteSpace(ctx.Saga.PostId))
                    {
                        await ctx.Publish(new PostDeletedEvent
                        {
                            PostId = ctx.Saga.PostId,
                            UserId = ctx.Saga.UserId,
                            DeletedAt = DateTime.UtcNow
                        });
                    }
                })
                .ThenAsync(ctx => PublishOrphanedFileEventsAsync(ctx))
                .PublishAsync(ctx => ctx.Init<PostWithMediaSagaCompletedEvent>(new PostWithMediaSagaCompletedEvent
                {
                    CorrelationId = ctx.Message.CorrelationId,
                    UserId = ctx.Saga.UserId,
                    PostId = null,
                    IsSuccess = false,
                    ErrorMessage = ctx.Message.Reason
                }))
                .Finalize());

        During(AttachingMedia,
            When(MediaAttachedToPost)
                .PublishAsync(ctx => ctx.Init<PostWithMediaSagaCompletedEvent>(new PostWithMediaSagaCompletedEvent
                {
                    CorrelationId = ctx.Message.CorrelationId,
                    UserId = ctx.Saga.UserId,
                    PostId = ctx.Message.PostId,
                    IsSuccess = true,
                    ErrorMessage = null
                }))
                .Finalize());

        SetCompletedWhenFinalized();
    }

    private static async Task PublishOrphanedFileEventsAsync(BehaviorContext<PostMediaSagaState> context)
    {
        foreach (var fileId in context.Saga.MediaFileIds)
        {
            await context.Publish(new FileOrphanedEvent
            {
                FileId = fileId
            });
        }
    }
}
