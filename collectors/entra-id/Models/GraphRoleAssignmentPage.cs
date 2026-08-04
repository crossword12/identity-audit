using System.Text.Json.Serialization;

namespace IdentityAudit.EntraCollector.Models;

public sealed class GraphRoleAssignmentPage
{
    [JsonPropertyName("@odata.context")]
    public string? ODataContext { get; init; }

    [JsonPropertyName("@odata.nextLink")]
    public string? ODataNextLink { get; init; }

    [JsonPropertyName("value")]
    public IReadOnlyCollection<GraphRoleAssignment> Value
    {
        get;
        init;
    } = Array.Empty<GraphRoleAssignment>();
}