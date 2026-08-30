using IdentityAudit.Application.AuditLogs;
using IdentityAudit.Domain.Enums;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AuditLogEntity =
    IdentityAudit.Domain.Entities.AuditLog;

namespace IdentityAudit.Infrastructure.Services;

public sealed class AuditLogService
    : IAuditLogService
{
    private readonly IdentityAuditDbContext _dbContext;

    public AuditLogService(
        IdentityAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordAsync(
        Guid? auditId,
        Guid? applicationUserId,
        AuditLogLevel level,
        string eventType,
        string message,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            eventType);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            message);

        var normalizedEventType =
            eventType.Trim();

        if (normalizedEventType.Length > 100)
        {
            throw new ArgumentException(
                "Le type d'événement ne peut pas dépasser 100 caractères.",
                nameof(eventType));
        }

        var auditLog = new AuditLogEntity
        {
            AuditId = auditId,
            ApplicationUserId = applicationUserId,
            Level = level,
            EventType = normalizedEventType,
            Message = message.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>?>
        GetByAuditAsync(
            Guid auditId,
            CancellationToken cancellationToken = default)
    {
        var auditExists =
            await _dbContext.Audits
                .AsNoTracking()
                .AnyAsync(
                    audit => audit.Id == auditId,
                    cancellationToken);

        if (!auditExists)
        {
            return null;
        }

        return await (
            from auditLog in
                _dbContext.AuditLogs.AsNoTracking()

            join applicationUser in
                _dbContext.Users.AsNoTracking()
                on auditLog.ApplicationUserId
                equals (Guid?)applicationUser.Id
                into matchingUsers

            from applicationUser in
                matchingUsers.DefaultIfEmpty()

            where auditLog.AuditId == auditId

            orderby auditLog.CreatedAt descending

            select new AuditLogDto(
                auditLog.Id,
                auditLog.AuditId,
                auditLog.ApplicationUserId,
                applicationUser == null
                    ? null
                    : applicationUser.DisplayName,
                applicationUser == null
                    ? null
                    : applicationUser.Email,
                auditLog.Level,
                auditLog.EventType,
                auditLog.Message,
                auditLog.CreatedAt)
        ).ToListAsync(cancellationToken);
    }
}