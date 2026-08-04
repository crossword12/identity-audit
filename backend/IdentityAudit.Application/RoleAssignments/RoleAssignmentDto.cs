namespace IdentityAudit.Application.RoleAssignments;

public sealed class RoleAssignmentDto
{
    public Guid Id { get; init; }

    public Guid AuditId { get; init; }

    public Guid IdentityId { get; init; }

    public string IdentityExternalId { get; init; }
        = string.Empty;

    public string IdentityDisplayName { get; init; }
        = string.Empty;

    public Guid DirectoryRoleId { get; init; }

    public string RoleExternalId { get; init; }
        = string.Empty;

    public string RoleName { get; init; }
        = string.Empty;

    public bool RoleIsPrivileged { get; init; }

    public DateTimeOffset? AssignedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public bool IsPermanent { get; init; }

    public DateTimeOffset CollectedAt { get; init; }
}