using System.Text.Json.Serialization;

namespace IdentityAudit.EntraCollector.Models;

public sealed class GraphRoleDefinitionPage
{
    [JsonPropertyName("@odata.context")]
    public string? ODataContext { get; init; }

    [JsonPropertyName("@odata.nextLink")]
    public string? ODataNextLink { get; init; }

    [JsonPropertyName("value")]
    public IReadOnlyCollection<GraphRoleDefinition> Value
    {
        get;
        init;
    } = Array.Empty<GraphRoleDefinition>();
}