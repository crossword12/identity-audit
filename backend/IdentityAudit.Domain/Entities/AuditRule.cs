using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Domain.Entities;

public sealed class AuditRule
{
    public Guid Id { get; set; }

    public string Code { get; set; }
        = string.Empty;

    public string Name { get; set; }
        = string.Empty;

    public string Description { get; set; }
        = string.Empty;

    public string CisControl { get; set; }
        = string.Empty;

    public TargetType TargetType { get; set; }

    public RuleSeverity Severity { get; set; }

    public string Recommendation { get; set; }
        = string.Empty;

    public bool IsEnabled { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<RuleEvaluation> Evaluations { get; set; }
        = new List<RuleEvaluation>();
}