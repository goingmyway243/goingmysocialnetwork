using GoingMy.Shared.Events;
using MassTransit;

namespace GoingMy.Upload.Application.Saga;

public class PostMediaSagaState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public int Version { get; set; }

    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Content { get; set; } = null!;
    public List<string> MediaFileIds { get; set; } = [];
    public List<MediaFileInfo> ValidatedMediaFiles { get; set; } = [];
    public string? PostId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
