using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? AuditId { get; set; }

    public Audit? Audit { get; set; }

    public Guid? ApplicationUserId { get; set; }

    public AuditLogLevel Level { get; set; }
        = AuditLogLevel.Information;

    public string EventType { get; set; }
        = string.Empty;

    public string Message { get; set; }
        = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
        = DateTimeOffset.UtcNow;
}