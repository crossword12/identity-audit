namespace IdentityAudit.Domain.Enums;

public enum AuditStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    CompletedWithWarnings = 4,
    Failed = 5
}