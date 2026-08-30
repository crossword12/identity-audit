namespace IdentityAudit.Application.AuditLogs;

public static class AuditLogEventTypes
{
    public const string AuditCreated =
        nameof(AuditCreated);

    public const string AuditStarted =
        nameof(AuditStarted);

    public const string AuditCompleted =
        nameof(AuditCompleted);

    public const string AuditRulesEvaluated =
        nameof(AuditRulesEvaluated);

    public const string AuditExported =
        nameof(AuditExported);
}