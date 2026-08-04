using IdentityAudit.Application.Roles;
using IdentityAudit.Domain.Enums;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using DirectoryRoleEntity =
    IdentityAudit.Domain.Entities.DirectoryRole;

namespace IdentityAudit.Infrastructure.Services;

public sealed class DirectoryRoleService
    : IDirectoryRoleService
{
    private readonly IdentityAuditDbContext _dbContext;

    public DirectoryRoleService(
        IdentityAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DirectoryRoleDto>>
        GetByAuditIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.DirectoryRoles
            .AsNoTracking()
            .Where(role => role.AuditId == auditId)
            .OrderByDescending(role => role.IsPrivileged)
            .ThenBy(role => role.Name)
            .Select(role => new DirectoryRoleDto
            {
                Id = role.Id,
                AuditId = role.AuditId,
                ExternalId = role.ExternalId,
                Name = role.Name,
                Description = role.Description,
                Source = role.Source.ToString(),
                IsPrivileged = role.IsPrivileged,
                CollectedAt = role.CollectedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ImportRolesResult> ImportAsync(
        Guid auditId,
        ImportRolesRequest request,
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
            return ImportRolesResult.Failure(
                "L'audit demandé est introuvable.");
        }

        if (audit.Status is AuditStatus.Completed
            or AuditStatus.CompletedWithWarnings
            or AuditStatus.Failed)
        {
            return ImportRolesResult.Failure(
                "Il est impossible d'importer des rôles dans un audit terminé.");
        }

        if (request.Roles.Count == 0)
        {
            return ImportRolesResult.Failure(
                "La liste des rôles est vide.");
        }

        var normalizedRoles = request.Roles
            .Select(role => new
            {
                ExternalId = role.ExternalId.Trim(),

                Name = string.IsNullOrWhiteSpace(role.Name)
                    ? role.ExternalId.Trim()
                    : role.Name.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(role.Description)
                        ? null
                        : role.Description.Trim(),

                Source = role.Source.Trim(),

                role.IsPrivileged
            })
            .ToList();

        if (normalizedRoles.Any(role =>
                role.ExternalId.Length == 0))
        {
            return ImportRolesResult.Failure(
                "Un rôle contient un identifiant externe vide.");
        }

        var duplicateRole = normalizedRoles
            .GroupBy(
                role => role.ExternalId,
                StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateRole is not null)
        {
            return ImportRolesResult.Failure(
                $"Le rôle '{duplicateRole.Key}' apparaît plusieurs fois dans la requête.");
        }

        var expectedSource =
            audit.Target.Type.ToString();

        var invalidSource = normalizedRoles
            .FirstOrDefault(role =>
                !role.Source.Equals(
                    expectedSource,
                    StringComparison.OrdinalIgnoreCase));

        if (invalidSource is not null)
        {
            return ImportRolesResult.Failure(
                $"La source '{invalidSource.Source}' ne correspond pas à la cible '{expectedSource}'.");
        }

        var existingRoles =
            await _dbContext.DirectoryRoles
                .AsNoTracking()
                .Where(role => role.AuditId == auditId)
                .ToListAsync(cancellationToken);

        var existingExternalIds = existingRoles
            .Select(role => role.ExternalId)
            .ToHashSet(
                StringComparer.OrdinalIgnoreCase);

        var alreadyExisting = normalizedRoles
            .FirstOrDefault(role =>
                existingExternalIds.Contains(
                    role.ExternalId));

        if (alreadyExisting is not null)
        {
            return ImportRolesResult.Failure(
                $"Le rôle '{alreadyExisting.ExternalId}' existe déjà dans cet audit.");
        }

        var collectedAt = DateTimeOffset.UtcNow;

        var entities = normalizedRoles
            .Select(role => new DirectoryRoleEntity
            {
                Id = Guid.NewGuid(),
                AuditId = auditId,
                ExternalId = role.ExternalId,
                Name = role.Name,
                Description = role.Description,
                Source = audit.Target.Type,
                IsPrivileged = role.IsPrivileged,
                CollectedAt = collectedAt
            })
            .ToList();

        await _dbContext.DirectoryRoles.AddRangeAsync(
            entities,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var importedRoles = entities
            .Select(role => new DirectoryRoleDto
            {
                Id = role.Id,
                AuditId = role.AuditId,
                ExternalId = role.ExternalId,
                Name = role.Name,
                Description = role.Description,
                Source = role.Source.ToString(),
                IsPrivileged = role.IsPrivileged,
                CollectedAt = role.CollectedAt
            })
            .ToList();

        return ImportRolesResult.Success(importedRoles);
    }
}