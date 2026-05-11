namespace GoingMy.Shared.Events;

public record CommentAddedEvent(
    string PostId,
    string PostAuthorUserId,
    string CommenterId,
    string CommenterUsername,
    string CommentPreview);
