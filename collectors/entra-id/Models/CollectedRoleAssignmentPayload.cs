namespace IdentityAudit.EntraCollector.Models;

public sealed class CollectedRoleAssignmentPayload
{
    public string IdentityExternalId { get; init; }
        = string.Empty;

    public string RoleExternalId { get; init; }
        = string.Empty;

    public DateTimeOffset? AssignedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public bool IsPermanent { get; init; }
}