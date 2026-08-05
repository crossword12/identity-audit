using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Application.RuleEvaluations;

public sealed class RuleEvaluationDto
{
    public Guid Id { get; init; }

    public Guid AuditId { get; init; }

    public Guid AuditRuleId { get; init; }

    public string RuleCode { get; init; }
        = string.Empty;

    public string RuleName { get; init; }
        = string.Empty;

    public string CisControl { get; init; }
        = string.Empty;

    public TargetType TargetType { get; init; }

    public RuleSeverity Severity { get; init; }

    public RuleEvaluationStatus Status { get; init; }

    public int FindingCount { get; init; }

    public string? EvidenceJson { get; init; }

    public string? Recommendation { get; init; }

    public string? ErrorMessage { get; init; }

    public DateTimeOffset EvaluatedAt { get; init; }
}