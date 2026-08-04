using IdentityAudit.Application.RoleAssignments;
using IdentityAudit.Domain.Enums;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using RoleAssignmentEntity =
    IdentityAudit.Domain.Entities.RoleAssignment;

namespace IdentityAudit.Infrastructure.Services;

public sealed class RoleAssignmentService
    : IRoleAssignmentService
{
    private readonly IdentityAuditDbContext _dbContext;

    public RoleAssignmentService(
        IdentityAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RoleAssignmentDto>>
        GetByAuditIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoleAssignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.Identity.AuditId == auditId
                && assignment.DirectoryRole.AuditId == auditId)
            .OrderByDescending(assignment =>
                assignment.DirectoryRole.IsPrivileged)
            .ThenBy(assignment =>
                assignment.DirectoryRole.Name)
            .ThenBy(assignment =>
                assignment.Identity.DisplayName)
            .Select(assignment =>
                new RoleAssignmentDto
                {
                    Id = assignment.Id,
                    AuditId = auditId,

                    IdentityId =
                        assignment.IdentityId,

                    IdentityExternalId =
                        assignment.Identity.ExternalId,

                    IdentityDisplayName =
                        assignment.Identity.DisplayName,

                    DirectoryRoleId =
                        assignment.DirectoryRoleId,

                    RoleExternalId =
                        assignment.DirectoryRole.ExternalId,

                    RoleName =
                        assignment.DirectoryRole.Name,

                    RoleIsPrivileged =
                        assignment.DirectoryRole.IsPrivileged,

                    AssignedAt =
                        assignment.AssignedAt,

                    ExpiresAt =
                        assignment.ExpiresAt,

                    IsPermanent =
                        assignment.IsPermanent,

                    CollectedAt =
                        assignment.CollectedAt
                })
            .ToListAsync(cancellationToken);
    }

    public async Task<ImportRoleAssignmentsResult>
        ImportAsync(
            Guid auditId,
            ImportRoleAssignmentsRequest request,
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
            return ImportRoleAssignmentsResult.Failure(
                "L'audit demandé est introuvable.");
        }

        if (audit.Status is AuditStatus.Completed
            or AuditStatus.CompletedWithWarnings
            or AuditStatus.Failed)
        {
            return ImportRoleAssignmentsResult.Failure(
                "Il est impossible d'importer des affectations dans un audit terminé.");
        }

        if (request.Assignments.Count == 0)
        {
            return ImportRoleAssignmentsResult.Failure(
                "La liste des affectations est vide.");
        }

        var normalizedAssignments = request.Assignments
            .Select(assignment => new
            {
                IdentityExternalId =
                    assignment.IdentityExternalId.Trim(),

                RoleExternalId =
                    assignment.RoleExternalId.Trim(),

                assignment.AssignedAt,
                assignment.ExpiresAt,
                assignment.IsPermanent
            })
            .ToList();

        var invalidAssignment =
            normalizedAssignments.FirstOrDefault(
                assignment =>
                    assignment.IdentityExternalId.Length == 0
                    || assignment.RoleExternalId.Length == 0);

        if (invalidAssignment is not null)
        {
            return ImportRoleAssignmentsResult.Failure(
                "Une affectation contient un identifiant externe vide.");
        }

        var permanentWithExpiration =
            normalizedAssignments.FirstOrDefault(
                assignment =>
                    assignment.IsPermanent
                    && assignment.ExpiresAt.HasValue);

        if (permanentWithExpiration is not null)
        {
            return ImportRoleAssignmentsResult.Failure(
                "Une affectation permanente ne peut pas avoir de date d'expiration.");
        }

        var invalidDates =
            normalizedAssignments.FirstOrDefault(
                assignment =>
                    assignment.AssignedAt.HasValue
                    && assignment.ExpiresAt.HasValue
                    && assignment.ExpiresAt.Value
                        < assignment.AssignedAt.Value);

        if (invalidDates is not null)
        {
            return ImportRoleAssignmentsResult.Failure(
                "La date d'expiration d'une affectation ne peut pas précéder sa date d'attribution.");
        }

        var duplicatedAssignment =
            normalizedAssignments
                .GroupBy(
                    assignment =>
                        $"{assignment.IdentityExternalId}" +
                        "\u001F" +
                        $"{assignment.RoleExternalId}",
                    StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group =>
                    group.Count() > 1);

        if (duplicatedAssignment is not null)
        {
            var duplicatedItem =
                duplicatedAssignment.First();

            return ImportRoleAssignmentsResult.Failure(
                $"L'affectation entre l'identité " +
                $"'{duplicatedItem.IdentityExternalId}' " +
                $"et le rôle " +
                $"'{duplicatedItem.RoleExternalId}' " +
                "apparaît plusieurs fois dans la requête.");
        }

        var identities = await _dbContext.Identities
            .Where(identity =>
                identity.AuditId == auditId)
            .ToListAsync(cancellationToken);

        var identitiesByExternalId =
            identities.ToDictionary(
                identity => identity.ExternalId,
                StringComparer.OrdinalIgnoreCase);

        var roles = await _dbContext.DirectoryRoles
            .AsNoTracking()
            .Where(role =>
                role.AuditId == auditId)
            .ToListAsync(cancellationToken);

        var rolesByExternalId =
            roles.ToDictionary(
                role => role.ExternalId,
                StringComparer.OrdinalIgnoreCase);

        foreach (var assignment in normalizedAssignments)
        {
            if (!identitiesByExternalId.ContainsKey(
                    assignment.IdentityExternalId))
            {
                return ImportRoleAssignmentsResult.Failure(
                    $"L'identité " +
                    $"'{assignment.IdentityExternalId}' " +
                    "n'existe pas dans cet audit.");
            }

            if (!rolesByExternalId.ContainsKey(
                    assignment.RoleExternalId))
            {
                return ImportRoleAssignmentsResult.Failure(
                    $"Le rôle " +
                    $"'{assignment.RoleExternalId}' " +
                    "n'existe pas dans cet audit.");
            }
        }

        var requestedPairs = normalizedAssignments
            .Select(assignment =>
            {
                var identity =
                    identitiesByExternalId[
                        assignment.IdentityExternalId];

                var role =
                    rolesByExternalId[
                        assignment.RoleExternalId];

                return new
                {
                    Request = assignment,
                    Identity = identity,
                    Role = role
                };
            })
            .ToList();

        var identityIds = requestedPairs
            .Select(item => item.Identity.Id)
            .Distinct()
            .ToList();

        var roleIds = requestedPairs
            .Select(item => item.Role.Id)
            .Distinct()
            .ToList();

        var existingAssignments =
            await _dbContext.RoleAssignments
                .AsNoTracking()
                .Where(assignment =>
                    identityIds.Contains(
                        assignment.IdentityId)
                    && roleIds.Contains(
                        assignment.DirectoryRoleId))
                .ToListAsync(cancellationToken);

        var existingAssignmentSet =
            existingAssignments
                .Select(assignment =>
                    $"{assignment.IdentityId:N}:" +
                    $"{assignment.DirectoryRoleId:N}")
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        foreach (var pair in requestedPairs)
        {
            var key =
                $"{pair.Identity.Id:N}:" +
                $"{pair.Role.Id:N}";

            if (existingAssignmentSet.Contains(key))
            {
                return ImportRoleAssignmentsResult.Failure(
                    $"L'identité " +
                    $"'{pair.Identity.ExternalId}' " +
                    "possède déjà le rôle " +
                    $"'{pair.Role.ExternalId}' " +
                    "dans cet audit.");
            }
        }

        var collectedAt = DateTimeOffset.UtcNow;

        var entities = requestedPairs
            .Select(pair =>
                new RoleAssignmentEntity
                {
                    Id = Guid.NewGuid(),

                    IdentityId =
                        pair.Identity.Id,

                    DirectoryRoleId =
                        pair.Role.Id,

                    AssignedAt =
                        pair.Request.AssignedAt,

                    ExpiresAt =
                        pair.Request.ExpiresAt,

                    IsPermanent =
                        pair.Request.IsPermanent,

                    CollectedAt =
                        collectedAt
                })
            .ToList();

        foreach (var pair in requestedPairs)
        {
            var assignmentIsActive =
                !pair.Request.ExpiresAt.HasValue
                || pair.Request.ExpiresAt.Value
                    > collectedAt;

            if (pair.Role.IsPrivileged
                && assignmentIsActive)
            {
                pair.Identity.IsPrivileged = true;
            }
        }

        await _dbContext.RoleAssignments
            .AddRangeAsync(
                entities,
                cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var importedAssignments = entities
            .Select((entity, index) =>
            {
                var pair = requestedPairs[index];

                return new RoleAssignmentDto
                {
                    Id = entity.Id,
                    AuditId = auditId,

                    IdentityId =
                        pair.Identity.Id,

                    IdentityExternalId =
                        pair.Identity.ExternalId,

                    IdentityDisplayName =
                        pair.Identity.DisplayName,

                    DirectoryRoleId =
                        pair.Role.Id,

                    RoleExternalId =
                        pair.Role.ExternalId,

                    RoleName =
                        pair.Role.Name,

                    RoleIsPrivileged =
                        pair.Role.IsPrivileged,

                    AssignedAt =
                        entity.AssignedAt,

                    ExpiresAt =
                        entity.ExpiresAt,

                    IsPermanent =
                        entity.IsPermanent,

                    CollectedAt =
                        entity.CollectedAt
                };
            })
            .ToList();

        return ImportRoleAssignmentsResult.Success(
            importedAssignments);
    }
}