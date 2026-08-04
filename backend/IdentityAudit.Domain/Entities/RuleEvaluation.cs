using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Domain.Entities;

public sealed class RuleEvaluation
{
    public Guid Id { get; set; }

    public Guid AuditId { get; set; }

    public Audit Audit { get; set; }
        = null!;

    public Guid AuditRuleId { get; set; }

    public AuditRule AuditRule { get; set; }
        = null!;

    public RuleEvaluationStatus Status { get; set; }

    public int FindingCount { get; set; }

    public string? EvidenceJson { get; set; }

    public string? Recommendation { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset EvaluatedAt { get; set; }
}