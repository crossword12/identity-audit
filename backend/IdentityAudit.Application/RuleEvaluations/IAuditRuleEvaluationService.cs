namespace IdentityAudit.Application.RuleEvaluations;

public interface IAuditRuleEvaluationService
{
    Task<EvaluateAuditRulesResult> EvaluateAsync(
        Guid auditId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RuleEvaluationDto>> GetByAuditAsync(
        Guid auditId,
        CancellationToken cancellationToken = default);
}