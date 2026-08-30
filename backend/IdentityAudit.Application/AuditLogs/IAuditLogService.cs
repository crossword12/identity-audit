using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Application.AuditLogs;

public interface IAuditLogService
{
    Task RecordAsync(
        Guid? auditId,
        Guid? applicationUserId,
        AuditLogLevel level,
        string eventType,
        string message,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>?> GetByAuditAsync(
        Guid auditId,
        CancellationToken cancellationToken = default);
}