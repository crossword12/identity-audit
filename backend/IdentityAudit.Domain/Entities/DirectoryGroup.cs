using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Domain.Entities;

public sealed class DirectoryGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AuditId { get; set; }

    public Audit Audit { get; set; } = null!;

    public string ExternalId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TargetType Source { get; set; }

    public string? GroupType { get; set; }

    public bool IsPrivileged { get; set; }

    public DateTimeOffset CollectedAt { get; set; }
        = DateTimeOffset.UtcNow;

    public ICollection<GroupMembership> Memberships { get; set; }
        = new List<GroupMembership>();
}