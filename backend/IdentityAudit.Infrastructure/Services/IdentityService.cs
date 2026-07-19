using IdentityAudit.Application.Identities;
using IdentityAudit.Domain.Enums;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using DirectoryIdentityEntity =
    IdentityAudit.Domain.Entities.DirectoryIdentity;

namespace IdentityAudit.Infrastructure.Services;

public sealed class IdentityService : IIdentityService
{
    private readonly IdentityAuditDbContext _dbContext;

    public IdentityService(IdentityAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DirectoryIdentityDto>>
        GetByAuditIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Identities
            .AsNoTracking()
            .Where(identity => identity.AuditId == auditId)
            .OrderBy(identity => identity.DisplayName)
            .Select(identity => new DirectoryIdentityDto
            {
                Id = identity.Id,
                AuditId = identity.AuditId,
                ExternalId = identity.ExternalId,
                DisplayName = identity.DisplayName,
                UserName = identity.UserName,
                Email = identity.Email,
                Source = identity.Source,
                AccountType = identity.AccountType,
                IsEnabled = identity.IsEnabled,
                IsPrivileged = identity.IsPrivileged,
                IsServiceAccount = identity.IsServiceAccount,
                IsLocked = identity.IsLocked,
                LastSignInAt = identity.LastSignInAt,
                Description = identity.Description,
                Owner = identity.Owner,
                CollectedAt = identity.CollectedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ImportIdentitiesResult> ImportAsync(
        Guid auditId,
        ImportIdentitiesRequest request,
        CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits
            .Include(currentAudit => currentAudit.Target)
            .SingleOrDefaultAsync(
                currentAudit => currentAudit.Id == auditId,
                cancellationToken);

        if (audit is null)
        {
            return ImportIdentitiesResult.Failure(
                "L'audit demandé est introuvable.");
        }

        if (audit.Status is AuditStatus.Completed
            or AuditStatus.CompletedWithWarnings
            or AuditStatus.Failed)
        {
            return ImportIdentitiesResult.Failure(
                "Il est impossible d'importer des identités dans un audit terminé.");
        }

        if (request.Identities.Count == 0)
        {
            return ImportIdentitiesResult.Failure(
                "La liste des identités est vide.");
        }

        var requestedExternalIds = request.Identities
            .Select(identity => identity.ExternalId.Trim())
            .ToList();

        var duplicatedExternalId = requestedExternalIds
            .GroupBy(
                externalId => externalId,
                StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicatedExternalId is not null)
        {
            return ImportIdentitiesResult.Failure(
                $"L'identité '{duplicatedExternalId.Key}' apparaît plusieurs fois dans la requête.");
        }

        var invalidSource = request.Identities
            .FirstOrDefault(identity =>
                identity.Source != audit.Target.Type);

        if (invalidSource is not null)
        {
            return ImportIdentitiesResult.Failure(
                $"La source '{invalidSource.Source}' ne correspond pas au type de la cible '{audit.Target.Type}'.");
        }

        var existingExternalIds = await _dbContext.Identities
            .AsNoTracking()
            .Where(identity => identity.AuditId == auditId)
            .Select(identity => identity.ExternalId)
            .ToListAsync(cancellationToken);

        var existingExternalIdSet = new HashSet<string>(
            existingExternalIds,
            StringComparer.OrdinalIgnoreCase);

        var alreadyExistingExternalId = requestedExternalIds
            .FirstOrDefault(existingExternalIdSet.Contains);

        if (alreadyExistingExternalId is not null)
        {
            return ImportIdentitiesResult.Failure(
                $"L'identité '{alreadyExistingExternalId}' existe déjà dans cet audit.");
        }

        var collectedAt = DateTimeOffset.UtcNow;

        var entities = request.Identities
            .Select(identity => new DirectoryIdentityEntity
            {
                Id = Guid.NewGuid(),
                AuditId = auditId,
                ExternalId = identity.ExternalId.Trim(),
                DisplayName = identity.DisplayName.Trim(),
                UserName = identity.UserName.Trim(),
                Email = NormalizeOptionalText(identity.Email),
                Source = identity.Source,
                AccountType = identity.AccountType,
                IsEnabled = identity.IsEnabled,
                IsPrivileged = identity.IsPrivileged,
                IsServiceAccount = identity.IsServiceAccount,
                IsLocked = identity.IsLocked,
                LastSignInAt = identity.LastSignInAt,
                Description = NormalizeOptionalText(
                    identity.Description),
                Owner = NormalizeOptionalText(identity.Owner),
                CollectedAt = collectedAt
            })
            .ToList();

        await _dbContext.Identities.AddRangeAsync(
            entities,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var importedIdentities = entities
            .Select(ToDto)
            .ToList();

        return ImportIdentitiesResult.Success(importedIdentities);
    }

    private static DirectoryIdentityDto ToDto(
        DirectoryIdentityEntity identity)
    {
        return new DirectoryIdentityDto
        {
            Id = identity.Id,
            AuditId = identity.AuditId,
            ExternalId = identity.ExternalId,
            DisplayName = identity.DisplayName,
            UserName = identity.UserName,
            Email = identity.Email,
            Source = identity.Source,
            AccountType = identity.AccountType,
            IsEnabled = identity.IsEnabled,
            IsPrivileged = identity.IsPrivileged,
            IsServiceAccount = identity.IsServiceAccount,
            IsLocked = identity.IsLocked,
            LastSignInAt = identity.LastSignInAt,
            Description = identity.Description,
            Owner = identity.Owner,
            CollectedAt = identity.CollectedAt
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}