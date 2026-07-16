namespace IdentityAudit.Application.Audits;

public sealed record CreateAuditResult(
    bool IsSuccess,
    AuditDto? Audit,
    string? Error)
{
    public static CreateAuditResult Success(AuditDto audit)
    {
        return new CreateAuditResult(true, audit, null);
    }

    public static CreateAuditResult Failure(string error)
    {
        return new CreateAuditResult(false, null, error);
    }
}