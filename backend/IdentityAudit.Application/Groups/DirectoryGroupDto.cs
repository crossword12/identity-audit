using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Application.Groups;

public sealed class DirectoryGroupDto
{
    public Guid Id { get; init; }

    public Guid AuditId { get; init; }

    public string ExternalId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public TargetType Source { get; init; }

    public string? GroupType { get; init; }

    public bool IsPrivileged { get; init; }

    public DateTimeOffset CollectedAt { get; init; }
}