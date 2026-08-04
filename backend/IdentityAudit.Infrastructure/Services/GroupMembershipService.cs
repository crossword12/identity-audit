using IdentityAudit.Application.GroupMemberships;
using IdentityAudit.Domain.Enums;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using GroupMembershipEntity =
    IdentityAudit.Domain.Entities.GroupMembership;

namespace IdentityAudit.Infrastructure.Services;

public sealed class GroupMembershipService
    : IGroupMembershipService
{
    private readonly IdentityAuditDbContext _dbContext;

    public GroupMembershipService(
        IdentityAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<GroupMembershipDto>>
        GetByAuditIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.GroupMemberships
            .AsNoTracking()
            .Where(membership =>
                membership.Identity.AuditId == auditId
                && membership.Group.AuditId == auditId)
            .OrderBy(membership =>
                membership.Group.Name)
            .ThenBy(membership =>
                membership.Identity.DisplayName)
            .Select(membership =>
                new GroupMembershipDto
                {
                    Id = membership.Id,
                    AuditId = auditId,

                    IdentityId =
                        membership.IdentityId,

                    IdentityExternalId =
                        membership.Identity.ExternalId,

                    IdentityDisplayName =
                        membership.Identity.DisplayName,

                    GroupId =
                        membership.GroupId,

                    GroupExternalId =
                        membership.Group.ExternalId,

                    GroupName =
                        membership.Group.Name,

                    MembershipType =
                        membership.MembershipType,

                    CollectedAt =
                        membership.CollectedAt
                })
            .ToListAsync(cancellationToken);
    }

    public async Task<ImportGroupMembershipsResult>
        ImportAsync(
            Guid auditId,
            ImportGroupMembershipsRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var audit = await _dbContext.Audits
            .AsNoTracking()
            .SingleOrDefaultAsync(
                currentAudit =>
                    currentAudit.Id == auditId,
                cancellationToken);

        if (audit is null)
        {
            return ImportGroupMembershipsResult.Failure(
                "L'audit demandé est introuvable.");
        }

        if (audit.Status is AuditStatus.Completed
            or AuditStatus.CompletedWithWarnings
            or AuditStatus.Failed)
        {
            return ImportGroupMembershipsResult.Failure(
                "Il est impossible d'importer des appartenances dans un audit terminé.");
        }

        if (request.Memberships.Count == 0)
        {
            return ImportGroupMembershipsResult.Failure(
                "La liste des appartenances est vide.");
        }

        var normalizedMemberships = request.Memberships
            .Select(membership => new
            {
                IdentityExternalId =
                    membership.IdentityExternalId.Trim(),

                GroupExternalId =
                    membership.GroupExternalId.Trim(),

                MembershipType =
                    string.IsNullOrWhiteSpace(
                        membership.MembershipType)
                        ? "Direct"
                        : membership.MembershipType.Trim()
            })
            .ToList();

        var invalidMembership =
            normalizedMemberships.FirstOrDefault(
                membership =>
                    membership.IdentityExternalId.Length == 0
                    || membership.GroupExternalId.Length == 0);

        if (invalidMembership is not null)
        {
            return ImportGroupMembershipsResult.Failure(
                "Une appartenance contient un identifiant externe vide.");
        }

        var duplicatedMembership =
            normalizedMemberships
                .GroupBy(
                    membership =>
                        $"{membership.IdentityExternalId}" +
                        "\u001F" +
                        $"{membership.GroupExternalId}",
                    StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group =>
                    group.Count() > 1);

        if (duplicatedMembership is not null)
        {
            var duplicatedItem =
                duplicatedMembership.First();

            return ImportGroupMembershipsResult.Failure(
                $"L'appartenance entre l'identité " +
                $"'{duplicatedItem.IdentityExternalId}' " +
                $"et le groupe " +
                $"'{duplicatedItem.GroupExternalId}' " +
                "apparaît plusieurs fois dans la requête.");
        }

        var identities = await _dbContext.Identities
            .AsNoTracking()
            .Where(identity =>
                identity.AuditId == auditId)
            .ToListAsync(cancellationToken);

        var identitiesByExternalId =
            identities.ToDictionary(
                identity => identity.ExternalId,
                StringComparer.OrdinalIgnoreCase);

        var groups = await _dbContext.DirectoryGroups
            .AsNoTracking()
            .Where(group =>
                group.AuditId == auditId)
            .ToListAsync(cancellationToken);

        var groupsByExternalId =
            groups.ToDictionary(
                group => group.ExternalId,
                StringComparer.OrdinalIgnoreCase);

        foreach (var membership in normalizedMemberships)
        {
            if (!identitiesByExternalId.ContainsKey(
                    membership.IdentityExternalId))
            {
                return ImportGroupMembershipsResult.Failure(
                    $"L'identité " +
                    $"'{membership.IdentityExternalId}' " +
                    "n'existe pas dans cet audit.");
            }

            if (!groupsByExternalId.ContainsKey(
                    membership.GroupExternalId))
            {
                return ImportGroupMembershipsResult.Failure(
                    $"Le groupe " +
                    $"'{membership.GroupExternalId}' " +
                    "n'existe pas dans cet audit.");
            }
        }

        var requestedPairs = normalizedMemberships
            .Select(membership =>
            {
                var identity =
                    identitiesByExternalId[
                        membership.IdentityExternalId];

                var group =
                    groupsByExternalId[
                        membership.GroupExternalId];

                return new
                {
                    Request = membership,
                    Identity = identity,
                    Group = group
                };
            })
            .ToList();

        var identityIds = requestedPairs
            .Select(item => item.Identity.Id)
            .Distinct()
            .ToList();

        var groupIds = requestedPairs
            .Select(item => item.Group.Id)
            .Distinct()
            .ToList();

        var existingMemberships =
            await _dbContext.GroupMemberships
                .AsNoTracking()
                .Where(membership =>
                    identityIds.Contains(
                        membership.IdentityId)
                    && groupIds.Contains(
                        membership.GroupId))
                .ToListAsync(cancellationToken);

        var existingMembershipSet =
            existingMemberships
                .Select(membership =>
                    $"{membership.IdentityId:N}:" +
                    $"{membership.GroupId:N}")
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        foreach (var pair in requestedPairs)
        {
            var key =
                $"{pair.Identity.Id:N}:" +
                $"{pair.Group.Id:N}";

            if (existingMembershipSet.Contains(key))
            {
                return ImportGroupMembershipsResult.Failure(
                    $"L'identité " +
                    $"'{pair.Identity.ExternalId}' " +
                    "appartient déjà au groupe " +
                    $"'{pair.Group.ExternalId}' " +
                    "dans cet audit.");
            }
        }

        var collectedAt = DateTimeOffset.UtcNow;

        var entities = requestedPairs
            .Select(pair =>
                new GroupMembershipEntity
                {
                    Id = Guid.NewGuid(),

                    IdentityId =
                        pair.Identity.Id,

                    GroupId =
                        pair.Group.Id,

                    MembershipType =
                        pair.Request.MembershipType,

                    CollectedAt =
                        collectedAt
                })
            .ToList();

        await _dbContext.GroupMemberships
            .AddRangeAsync(
                entities,
                cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var importedMemberships = entities
            .Select((entity, index) =>
            {
                var pair = requestedPairs[index];

                return new GroupMembershipDto
                {
                    Id = entity.Id,
                    AuditId = auditId,

                    IdentityId =
                        pair.Identity.Id,

                    IdentityExternalId =
                        pair.Identity.ExternalId,

                    IdentityDisplayName =
                        pair.Identity.DisplayName,

                    GroupId =
                        pair.Group.Id,

                    GroupExternalId =
                        pair.Group.ExternalId,

                    GroupName =
                        pair.Group.Name,

                    MembershipType =
                        entity.MembershipType,

                    CollectedAt =
                        entity.CollectedAt
                };
            })
            .ToList();

        return ImportGroupMembershipsResult.Success(
            importedMemberships);
    }
}