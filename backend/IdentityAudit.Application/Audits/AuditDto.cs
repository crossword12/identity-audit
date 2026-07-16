using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Application.Audits;

public sealed record AuditDto(
    Guid Id,
    Guid TargetId,
    string TargetName,
    AuditStatus Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    decimal? ComplianceScore,
    string? ErrorMessage,
    DateTimeOffset CreatedAt);