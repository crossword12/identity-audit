using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Domain.Entities;

public sealed class Audit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TargetId { get; set; }

    public Target Target { get; set; } = null!;

    public Guid? CreatedByUserId { get; set; }

    public AuditStatus Status { get; set; } = AuditStatus.Pending;

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public decimal? ComplianceScore { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<DirectoryIdentity> Identities { get; set; }
    = new List<DirectoryIdentity>();

    public ICollection<DirectoryGroup> Groups { get; set; }
    = new List<DirectoryGroup>();
}