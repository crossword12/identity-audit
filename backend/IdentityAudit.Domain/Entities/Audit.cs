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

    public ICollection<DirectoryRole> Roles { get; set; }
    = new List<DirectoryRole>();

    public ICollection<RuleEvaluation> RuleEvaluations { get; set; }
    = new List<RuleEvaluation>();

    public ICollection<AuditLog> AuditLogs { get; set; }
    = new List<AuditLog>();
}