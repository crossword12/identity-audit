namespace IdentityAudit.Application.RuleEvaluations;

public sealed class EvaluateAuditRulesResult
{
    public Guid AuditId { get; init; }

    public int EvaluatedRuleCount { get; init; }

    public int CompliantCount { get; init; }

    public int NonCompliantCount { get; init; }

    public int NotApplicableCount { get; init; }

    public int NotVerifiableCount { get; init; }

    public int ErrorCount { get; init; }

    public decimal? ComplianceScore { get; init; }

    public IReadOnlyCollection<RuleEvaluationDto> Evaluations
    {
        get;
        init;
    } = Array.Empty<RuleEvaluationDto>();
}