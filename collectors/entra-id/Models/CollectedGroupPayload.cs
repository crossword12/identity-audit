namespace IdentityAudit.EntraCollector.Models;

public sealed class CollectedGroupPayload
{
    public string ExternalId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string Source { get; init; } = "EntraId";

    public string GroupType { get; init; } = "Other";

    public bool IsPrivileged { get; init; }
}