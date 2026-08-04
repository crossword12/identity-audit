namespace IdentityAudit.EntraCollector.Models;

public sealed class CollectedRolePayload
{
    public string ExternalId { get; init; }
        = string.Empty;

    public string Name { get; init; }
        = string.Empty;

    public string? Description { get; init; }

    public string Source { get; init; }
        = "EntraId";

    public bool IsPrivileged { get; init; }
}