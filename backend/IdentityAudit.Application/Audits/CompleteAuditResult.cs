namespace IdentityAudit.Application.Audits;

public sealed class CompleteAuditResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public AuditDto? Audit { get; init; }

    public static CompleteAuditResult Success(AuditDto audit)
    {
        return new CompleteAuditResult
        {
            Succeeded = true,
            Audit = audit
        };
    }

    public static CompleteAuditResult Failure(string errorMessage)
    {
        return new CompleteAuditResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}