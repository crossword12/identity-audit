using IdentityAudit.Application.Targets;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TargetEntity = IdentityAudit.Domain.Entities.Target;

namespace IdentityAudit.Infrastructure.Services;

public sealed class TargetService : ITargetService
{
    private readonly IdentityAuditDbContext _dbContext;

    public TargetService(IdentityAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TargetDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Targets
            .AsNoTracking()
            .OrderBy(target => target.Name)
            .Select(target => new TargetDto(
                target.Id,
                target.Name,
                target.Type,
                target.IsEnabled,
                target.ConfigurationJson,
                target.CreatedAt,
                target.UpdatedAt,
                target.LastCollectedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TargetDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Targets
            .AsNoTracking()
            .Where(target => target.Id == id)
            .Select(target => new TargetDto(
                target.Id,
                target.Name,
                target.Type,
                target.IsEnabled,
                target.ConfigurationJson,
                target.CreatedAt,
                target.UpdatedAt,
                target.LastCollectedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TargetDto> CreateAsync(
        CreateTargetRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var target = new TargetEntity
        {
            Name = request.Name.Trim(),
            Type = request.Type,
            IsEnabled = request.IsEnabled,
            ConfigurationJson =
                string.IsNullOrWhiteSpace(request.ConfigurationJson)
                    ? null
                    : request.ConfigurationJson,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Targets.Add(target);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TargetDto(
            target.Id,
            target.Name,
            target.Type,
            target.IsEnabled,
            target.ConfigurationJson,
            target.CreatedAt,
            target.UpdatedAt,
            target.LastCollectedAt);
    }
}