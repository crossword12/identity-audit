using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Domain.Entities;

public sealed class DirectoryRole
{
    public Guid Id { get; set; }

    public Guid AuditId { get; set; }

    public Audit Audit { get; set; } = null!;

    public string ExternalId { get; set; }
        = string.Empty;

    public string Name { get; set; }
        = string.Empty;

    public string? Description { get; set; }

    public TargetType Source { get; set; }

    public bool IsPrivileged { get; set; }

    public DateTimeOffset CollectedAt { get; set; }

    public ICollection<RoleAssignment> Assignments { get; set; }
        = new List<RoleAssignment>();
}