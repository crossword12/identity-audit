using IdentityAudit.Application.Targets;
using IdentityAudit.Infrastructure.Persistence;
using System.Text.Json;
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
    public async Task<UpdateTargetResult> UpdateAsync(
    Guid targetId,
    UpdateTargetRequest request,
    CancellationToken cancellationToken)
    {
        var target = await _dbContext.Targets
            .FirstOrDefaultAsync(
                currentTarget => currentTarget.Id == targetId,
                cancellationToken);

        if (target is null)
        {
            return UpdateTargetResult.Failure(
                "La cible demandée est introuvable.");
        }

        target.Name = request.Name.Trim();
        target.IsEnabled = request.IsEnabled;
        target.ConfigurationJson =
            string.IsNullOrWhiteSpace(request.ConfigurationJson)
                ? null
                : request.ConfigurationJson.Trim();

        target.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var targetDto = new TargetDto(
            target.Id,
            target.Name,
            target.Type,
            target.IsEnabled,
            target.ConfigurationJson,
            target.CreatedAt,
            target.UpdatedAt,
            target.LastCollectedAt);

        return UpdateTargetResult.Success(targetDto);
    }
    public async Task<TestTargetConnectionResult> TestConnectionAsync(
    Guid targetId,
    CancellationToken cancellationToken)
    {
        var target = await _dbContext.Targets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                currentTarget => currentTarget.Id == targetId,
                cancellationToken);

        if (target is null)
        {
            return TestTargetConnectionResult.NotFound(
                "La cible demandée est introuvable.");
        }

        if (!target.IsEnabled)
        {
            return TestTargetConnectionResult.Failure(
                "Le test de connexion est impossible car la cible est désactivée.");
        }

        if (!string.IsNullOrWhiteSpace(target.ConfigurationJson))
        {
            try
            {
                using var configuration =
                    JsonDocument.Parse(target.ConfigurationJson);
            }
            catch (JsonException)
            {
                return TestTargetConnectionResult.Failure(
                    "La configuration JSON de la cible est invalide.");
            }
        }

        return TestTargetConnectionResult.Success(
            $"Test de connexion simulé réussi pour la cible " +
            $"« {target.Name} » de type {target.Type}.");
    }
}