namespace IdentityAudit.Application.Audits;

public sealed class StartAuditResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public AuditDto? Audit { get; init; }

    public static StartAuditResult Success(AuditDto audit)
    {
        return new StartAuditResult
        {
            Succeeded = true,
            Audit = audit
        };
    }

    public static StartAuditResult Failure(string errorMessage)
    {
        return new StartAuditResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}