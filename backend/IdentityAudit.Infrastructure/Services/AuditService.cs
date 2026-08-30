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
        Guid? createdByUserId,
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
            CreatedByUserId = createdByUserId,
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
    public async Task<StartAuditResult> StartAsync(
    Guid auditId,
    CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits
            .Include(currentAudit => currentAudit.Target)
            .SingleOrDefaultAsync(
                currentAudit => currentAudit.Id == auditId,
                cancellationToken);

        if (audit is null)
        {
            return StartAuditResult.Failure(
                "L'audit demandé est introuvable.");
        }

        if (!audit.Target.IsEnabled)
        {
            return StartAuditResult.Failure(
                "La cible associée à cet audit est désactivée.");
        }

        if (audit.Status != AuditStatus.Pending)
        {
            return StartAuditResult.Failure(
                $"L'audit ne peut pas être démarré car son statut actuel est '{audit.Status}'.");
        }

        audit.Status = AuditStatus.Running;
        audit.StartedAt = DateTimeOffset.UtcNow;
        audit.CompletedAt = null;
        audit.ErrorMessage = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return StartAuditResult.Success(ToDto(audit));
    }

    public async Task<CompleteAuditResult> CompleteAsync(
    Guid auditId,
    CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits
            .Include(currentAudit => currentAudit.Target)
            .SingleOrDefaultAsync(
                currentAudit => currentAudit.Id == auditId,
                cancellationToken);

        if (audit is null)
        {
            return CompleteAuditResult.Failure(
                "L'audit demandé est introuvable.");
        }

        if (audit.Status != AuditStatus.Running)
        {
            return CompleteAuditResult.Failure(
                $"L'audit ne peut pas être terminé car son statut actuel est '{audit.Status}'.");
        }

        var hasIdentities = await _dbContext.Identities
            .AsNoTracking()
            .AnyAsync(
                identity => identity.AuditId == auditId,
                cancellationToken);

        if (!hasIdentities)
        {
            return CompleteAuditResult.Failure(
                "L'audit ne peut pas être terminé car aucune identité n'a été collectée.");
        }

        var completedAt = DateTimeOffset.UtcNow;

        audit.Status = AuditStatus.Completed;
        audit.CompletedAt = completedAt;
        audit.ErrorMessage = null;

        audit.Target.LastCollectedAt = completedAt;
        audit.Target.UpdatedAt = completedAt;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CompleteAuditResult.Success(ToDto(audit));
    }
    private static AuditDto ToDto(AuditEntity audit)
    {
        return new AuditDto(
            audit.Id,
            audit.TargetId,
            audit.Target.Name,
            audit.Status,
            audit.StartedAt,
            audit.CompletedAt,
            audit.ComplianceScore,
            audit.ErrorMessage,
            audit.CreatedAt);
    }
}