using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Domain.Entities;

public sealed class DirectoryIdentity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AuditId { get; set; }

    public Audit Audit { get; set; } = null!;

    public string ExternalId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public TargetType Source { get; set; }

    public AccountType AccountType { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsPrivileged { get; set; }

    public bool IsServiceAccount { get; set; }

    public bool? IsLocked { get; set; }

    public DateTimeOffset? LastSignInAt { get; set; }

    public string? Description { get; set; }

    public string? Owner { get; set; }

    public DateTimeOffset CollectedAt { get; set; }
        = DateTimeOffset.UtcNow;

    public ICollection<GroupMembership> GroupMemberships { get; set; }
    = new List<GroupMembership>();
}