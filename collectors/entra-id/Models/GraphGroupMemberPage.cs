using System.Text.Json.Serialization;

namespace IdentityAudit.EntraCollector.Models;

public sealed class GraphGroupMemberPage
{
    [JsonPropertyName("@odata.context")]
    public string? ODataContext { get; init; }

    [JsonPropertyName("@odata.nextLink")]
    public string? ODataNextLink { get; init; }

    [JsonPropertyName("value")]
    public IReadOnlyCollection<GraphGroupMember> Value
    {
        get;
        init;
    } = Array.Empty<GraphGroupMember>();
}