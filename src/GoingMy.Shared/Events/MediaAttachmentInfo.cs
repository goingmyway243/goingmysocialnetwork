namespace GoingMy.Shared.Events;

/// <summary>
/// Full media attachment metadata carried in post events so consumers (e.g. SearchService)
/// can index and serve attachment data without calling back to PostService.
/// </summary>
public record MediaAttachmentInfo(
    string FileId,
    string Url,
    string ContentType,
    int? Width,
    int? Height);
