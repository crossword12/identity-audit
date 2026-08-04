using System.Text.Json.Serialization;

namespace IdentityAudit.EntraCollector.Models;

public sealed class GraphGroupPage
{
    [JsonPropertyName("@odata.context")]
    public string? ODataContext { get; init; }

    [JsonPropertyName("@odata.nextLink")]
    public string? ODataNextLink { get; init; }

    [JsonPropertyName("value")]
    public IReadOnlyCollection<GraphGroup> Value { get; init; }
        = Array.Empty<GraphGroup>();
}