namespace GoingMy.Chat.Application.Dtos;

/// <summary>
/// Records when a specific user read a message.
/// </summary>
public record ReadReceiptDto(
    string MessageId,
    string ReadByUserId,
    string ReadByUsername,
    DateTime ReadAt
);
