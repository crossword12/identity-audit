using IdentityAudit.Application.Groups;
using IdentityAudit.Domain.Enums;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using DirectoryGroupEntity =
    IdentityAudit.Domain.Entities.DirectoryGroup;

namespace IdentityAudit.Infrastructure.Services;

public sealed class DirectoryGroupService
    : IDirectoryGroupService
{
    private readonly IdentityAuditDbContext _dbContext;

    public DirectoryGroupService(
        IdentityAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DirectoryGroupDto>>
        GetByAuditIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.DirectoryGroups
            .AsNoTracking()
            .Where(group => group.AuditId == auditId)
            .OrderBy(group => group.Name)
            .Select(group => new DirectoryGroupDto
            {
                Id = group.Id,
                AuditId = group.AuditId,
                ExternalId = group.ExternalId,
                Name = group.Name,
                Description = group.Description,
                Source = group.Source,
                GroupType = group.GroupType,
                IsPrivileged = group.IsPrivileged,
                CollectedAt = group.CollectedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ImportGroupsResult> ImportAsync(
        Guid auditId,
        ImportGroupsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var audit = await _dbContext.Audits
            .Include(currentAudit => currentAudit.Target)
            .SingleOrDefaultAsync(
                currentAudit => currentAudit.Id == auditId,
                cancellationToken);

        if (audit is null)
        {
            return ImportGroupsResult.Failure(
                "L'audit demandé est introuvable.");
        }

        if (audit.Status is AuditStatus.Completed
            or AuditStatus.CompletedWithWarnings
            or AuditStatus.Failed)
        {
            return ImportGroupsResult.Failure(
                "Il est impossible d'importer des groupes dans un audit terminé.");
        }

        if (request.Groups.Count == 0)
        {
            return ImportGroupsResult.Failure(
                "La liste des groupes est vide.");
        }

        var requestedExternalIds = request.Groups
            .Select(group => group.ExternalId.Trim())
            .ToList();

        var duplicatedExternalId = requestedExternalIds
            .GroupBy(
                externalId => externalId,
                StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicatedExternalId is not null)
        {
            return ImportGroupsResult.Failure(
                $"Le groupe '{duplicatedExternalId.Key}' apparaît plusieurs fois dans la requête.");
        }

        var invalidSource = request.Groups
            .FirstOrDefault(group =>
                group.Source != audit.Target.Type);

        if (invalidSource is not null)
        {
            return ImportGroupsResult.Failure(
                $"La source '{invalidSource.Source}' ne correspond pas au type de la cible '{audit.Target.Type}'.");
        }

        var existingExternalIds =
            await _dbContext.DirectoryGroups
                .AsNoTracking()
                .Where(group => group.AuditId == auditId)
                .Select(group => group.ExternalId)
                .ToListAsync(cancellationToken);

        var existingExternalIdSet = new HashSet<string>(
            existingExternalIds,
            StringComparer.OrdinalIgnoreCase);

        var alreadyExistingExternalId = requestedExternalIds
            .FirstOrDefault(existingExternalIdSet.Contains);

        if (alreadyExistingExternalId is not null)
        {
            return ImportGroupsResult.Failure(
                $"Le groupe '{alreadyExistingExternalId}' existe déjà dans cet audit.");
        }

        var collectedAt = DateTimeOffset.UtcNow;

        var entities = request.Groups
            .Select(group => new DirectoryGroupEntity
            {
                Id = Guid.NewGuid(),
                AuditId = auditId,
                ExternalId = group.ExternalId.Trim(),
                Name = group.Name.Trim(),
                Description = NormalizeOptionalText(
                    group.Description),
                Source = group.Source,
                GroupType = NormalizeOptionalText(
                    group.GroupType),
                IsPrivileged = group.IsPrivileged,
                CollectedAt = collectedAt
            })
            .ToList();

        await _dbContext.DirectoryGroups.AddRangeAsync(
            entities,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var importedGroups = entities
            .Select(ToDto)
            .ToList();

        return ImportGroupsResult.Success(importedGroups);
    }

    private static DirectoryGroupDto ToDto(
        DirectoryGroupEntity group)
    {
        return new DirectoryGroupDto
        {
            Id = group.Id,
            AuditId = group.AuditId,
            ExternalId = group.ExternalId,
            Name = group.Name,
            Description = group.Description,
            Source = group.Source,
            GroupType = group.GroupType,
            IsPrivileged = group.IsPrivileged,
            CollectedAt = group.CollectedAt
        };
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}