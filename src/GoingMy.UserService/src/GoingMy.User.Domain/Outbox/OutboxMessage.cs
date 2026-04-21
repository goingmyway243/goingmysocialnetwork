namespace GoingMy.User.Domain.Outbox;

/// <summary>
/// Represents an outbox message stored atomically with business data.
/// A background worker reads pending entries and publishes them to RabbitMQ,
/// ensuring exactly-once delivery even in the face of transient failures.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Simple name of the event type (e.g. "UserCreatedEvent").</summary>
    public string EventType { get; set; } = null!;

    /// <summary>JSON-serialized event payload.</summary>
    public string Payload { get; set; } = null!;

    /// <summary>When the outbox entry was created (same transaction as the domain change).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Set when the message was successfully published to RabbitMQ.</summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>Last error message if publishing failed.</summary>
    public string? Error { get; set; }

    /// <summary>How many times publishing has been attempted unsuccessfully.</summary>
    public int RetryCount { get; set; }
}
