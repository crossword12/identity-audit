namespace IdentityAudit.Application.Roles;

public sealed class DirectoryRoleDto
{
    public Guid Id { get; init; }

    public Guid AuditId { get; init; }

    public string ExternalId { get; init; }
        = string.Empty;

    public string Name { get; init; }
        = string.Empty;

    public string? Description { get; init; }

    public string Source { get; init; }
        = string.Empty;

    public bool IsPrivileged { get; init; }

    public DateTimeOffset CollectedAt { get; init; }
}