namespace GoingMy.Search.API.Models;

/// <summary>
/// Payload for an Elasticsearch completion suggester field.
/// Serialized as { "input": [...] } to satisfy the completion field format.
/// </summary>
public class SuggestField
{
    public IEnumerable<string> Input { get; set; } = [];
    public int? Weight { get; set; }
}
