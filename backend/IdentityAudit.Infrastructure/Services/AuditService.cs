using IdentityAudit.Application.Audits;
using IdentityAudit.Domain.Enums;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AuditEntity = IdentityAudit.Domain.Entities.Audit;

namespace IdentityAudit.Infrastructure.Services;

public sealed class AuditService : IAuditService
{
    private readonly IdentityAuditDbContext _dbContext;

    public AuditService(IdentityAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AuditDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Audits
            .AsNoTracking()
            .OrderByDescending(audit => audit.CreatedAt)
            .Select(audit => new AuditDto(
                audit.Id,
                audit.TargetId,
                audit.Target.Name,
                audit.Status,
                audit.StartedAt,
                audit.CompletedAt,
                audit.ComplianceScore,
                audit.ErrorMessage,
                audit.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<AuditDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Audits
            .AsNoTracking()
            .Where(audit => audit.Id == id)
            .Select(audit => new AuditDto(
                audit.Id,
                audit.TargetId,
                audit.Target.Name,
                audit.Status,
                audit.StartedAt,
                audit.CompletedAt,
                audit.ComplianceScore,
                audit.ErrorMessage,
                audit.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CreateAuditResult> CreateAsync(
        CreateAuditRequest request,
        CancellationToken cancellationToken)
    {
        var target = await _dbContext.Targets
            .AsNoTracking()
            .Where(target => target.Id == request.TargetId)
            .Select(target => new
            {
                target.Id,
                target.Name,
                target.IsEnabled
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (target is null)
        {
            return CreateAuditResult.Failure(
                "La cible sélectionnée est introuvable.");
        }

        if (!target.IsEnabled)
        {
            return CreateAuditResult.Failure(
                "La cible sélectionnée est désactivée.");
        }

        var audit = new AuditEntity
        {
            TargetId = target.Id,
            Status = AuditStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Audits.Add(audit);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var auditDto = new AuditDto(
            audit.Id,
            audit.TargetId,
            target.Name,
            audit.Status,
            audit.StartedAt,
            audit.CompletedAt,
            audit.ComplianceScore,
            audit.ErrorMessage,
            audit.CreatedAt);

        return CreateAuditResult.Success(auditDto);
    }
}