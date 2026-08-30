using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Application.AuditLogs;

public sealed record AuditLogDto(
    Guid Id,
    Guid? AuditId,
    Guid? ApplicationUserId,
    string? ApplicationUserDisplayName,
    string? ApplicationUserEmail,
    AuditLogLevel Level,
    string EventType,
    string Message,
    DateTimeOffset CreatedAt);