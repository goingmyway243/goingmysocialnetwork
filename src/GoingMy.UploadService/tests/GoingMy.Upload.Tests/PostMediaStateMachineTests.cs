using GoingMy.Shared.Events;
using GoingMy.Upload.Application.Saga;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GoingMy.Upload.Tests;

public class PostMediaStateMachineTests
{
    [Fact]
    public async Task Should_publish_orphan_and_failed_completion_when_media_validation_fails()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<PostMediaStateMachine, PostMediaSagaState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var correlationId = Guid.NewGuid();
            var fileIds = new[] { "file-1", "file-2" };

            await harness.Bus.Publish(new PostWithMediaRequestedEvent
            {
                CorrelationId = correlationId,
                UserId = "user-1",
                Username = "alice",
                Content = "hello",
                MediaFileIds = fileIds
            });

            await harness.Bus.Publish(new MediaValidationFailedEvent
            {
                CorrelationId = correlationId,
                Reason = "invalid media"
            });

            Assert.True(await harness.Published.Any<FileOrphanedEvent>(x => x.Context.Message.FileId == "file-1"));
            Assert.True(await harness.Published.Any<FileOrphanedEvent>(x => x.Context.Message.FileId == "file-2"));
            Assert.True(await harness.Published.Any<PostWithMediaSagaCompletedEvent>(x =>
                x.Context.Message.CorrelationId == correlationId &&
                x.Context.Message.IsSuccess == false &&
                x.Context.Message.ErrorMessage == "invalid media"));
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Should_publish_orphan_and_failed_completion_when_post_creation_fails()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<PostMediaStateMachine, PostMediaSagaState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var correlationId = Guid.NewGuid();
            var fileIds = new[] { "file-a", "file-b" };

            await harness.Bus.Publish(new PostWithMediaRequestedEvent
            {
                CorrelationId = correlationId,
                UserId = "user-2",
                Username = "bob",
                Content = "content",
                MediaFileIds = fileIds
            });

            await harness.Bus.Publish(new MediaValidatedEvent
            {
                CorrelationId = correlationId,
                MediaFileIds = fileIds,
                MediaFiles =
                [
                    new MediaFileInfo("file-a", "https://cdn/a.jpg", "image/jpeg"),
                    new MediaFileInfo("file-b", "https://cdn/b.jpg", "image/jpeg")
                ]
            });

            await harness.Bus.Publish(new PostCreationFailedEvent
            {
                CorrelationId = correlationId,
                Reason = "post create failed"
            });

            Assert.True(await harness.Published.Any<FileOrphanedEvent>(x => x.Context.Message.FileId == "file-a"));
            Assert.True(await harness.Published.Any<FileOrphanedEvent>(x => x.Context.Message.FileId == "file-b"));
            Assert.True(await harness.Published.Any<PostWithMediaSagaCompletedEvent>(x =>
                x.Context.Message.CorrelationId == correlationId &&
                x.Context.Message.IsSuccess == false &&
                x.Context.Message.ErrorMessage == "post create failed"));
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Should_publish_success_completion_when_media_attached()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<PostMediaStateMachine, PostMediaSagaState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var correlationId = Guid.NewGuid();
            var fileIds = new[] { "file-x" };

            await harness.Bus.Publish(new PostWithMediaRequestedEvent
            {
                CorrelationId = correlationId,
                UserId = "user-3",
                Username = "charlie",
                Content = "content",
                MediaFileIds = fileIds
            });

            await harness.Bus.Publish(new MediaValidatedEvent
            {
                CorrelationId = correlationId,
                MediaFileIds = fileIds,
                MediaFiles = [new MediaFileInfo("file-x", "https://cdn/x.jpg", "image/jpeg")]
            });

            await harness.Bus.Publish(new PostCreatedForSagaEvent
            {
                CorrelationId = correlationId,
                PostId = "post-123"
            });

            await harness.Bus.Publish(new MediaAttachedToPostEvent
            {
                CorrelationId = correlationId,
                PostId = "post-123"
            });

            Assert.True(await harness.Published.Any<PostWithMediaSagaCompletedEvent>(x =>
                x.Context.Message.CorrelationId == correlationId &&
                x.Context.Message.IsSuccess &&
                x.Context.Message.PostId == "post-123"));
            Assert.False(await harness.Published.Any<FileOrphanedEvent>(x => x.Context.Message.FileId == "file-x"));
        }
        finally
        {
            await harness.Stop();
        }
    }
}
