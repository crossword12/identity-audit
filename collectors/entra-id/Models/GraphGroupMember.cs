using System.Text.Json.Serialization;

namespace IdentityAudit.EntraCollector.Models;

public sealed class GraphGroupMember
{
    [JsonPropertyName("@odata.type")]
    public string ODataType { get; init; } = string.Empty;

    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string? UserPrincipalName { get; init; }
}