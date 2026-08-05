using System.Text.Json;
using IdentityAudit.Application.RuleEvaluations;
using IdentityAudit.Domain.Enums;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AuditEntity = IdentityAudit.Domain.Entities.Audit;
using RuleEvaluationEntity =
    IdentityAudit.Domain.Entities.RuleEvaluation;

namespace IdentityAudit.Infrastructure.Services;

public sealed class AuditRuleEvaluationService
    : IAuditRuleEvaluationService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IdentityAuditDbContext _dbContext;

    public AuditRuleEvaluationService(
        IdentityAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EvaluateAuditRulesResult> EvaluateAsync(
        Guid auditId,
        CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits
            .Include(currentAudit => currentAudit.Target)
            .Include(currentAudit => currentAudit.Identities)
                .ThenInclude(identity =>
                    identity.GroupMemberships)
                .ThenInclude(membership =>
                    membership.Group)
            .Include(currentAudit => currentAudit.Identities)
                .ThenInclude(identity =>
                    identity.RoleAssignments)
                .ThenInclude(assignment =>
                    assignment.DirectoryRole)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                currentAudit =>
                    currentAudit.Id == auditId,
                cancellationToken);

        if (audit is null)
        {
            throw new KeyNotFoundException(
                $"L'audit {auditId} est introuvable.");
        }

        if (!audit.CompletedAt.HasValue)
        {
            throw new InvalidOperationException(
                "L'audit doit être finalisé avant " +
                "l'exécution des règles CIS.");
        }

        var rules = await _dbContext.AuditRules
            .Where(rule =>
                rule.IsEnabled &&
                rule.TargetType == audit.Target.Type)
            .OrderBy(rule => rule.Code)
            .ToListAsync(cancellationToken);

        if (rules.Count == 0)
        {
            audit.ComplianceScore = null;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return new EvaluateAuditRulesResult
            {
                AuditId = auditId,
                ComplianceScore = null
            };
        }

        var referenceTime =
            audit.CompletedAt.Value;

        var evaluatedAt =
            DateTimeOffset.UtcNow;

        var evaluations =
            new List<RuleEvaluationEntity>();

        foreach (var rule in rules)
        {
            EvaluationOutcome outcome;

            try
            {
                outcome = rule.Code switch
                {
                    "ENTRA-05-01" =>
                        EvaluateInactiveAccounts(
                            audit,
                            referenceTime),

                    "ENTRA-05-02" =>
                        EvaluateDisabledPrivilegedAccounts(
                            audit,
                            referenceTime),

                    "ENTRA-05-03" =>
                        EvaluateServiceAccountsWithoutOwner(
                            audit),

                    "ENTRA-06-01" =>
                        EvaluatePermanentPrivilegedAssignments(
                            audit,
                            referenceTime),

                    "ENTRA-06-02" =>
                        EvaluatePrivilegedGroupMemberships(
                            audit),

                    "ENTRA-06-03" =>
                        EvaluatePrivilegedGuestAccounts(
                            audit,
                            referenceTime),

                    _ => new EvaluationOutcome(
                        RuleEvaluationStatus.Error,
                        0,
                        null,
                        "Aucune implémentation n'existe " +
                        $"pour la règle {rule.Code}.")
                };
            }
            catch (Exception exception)
            {
                outcome = new EvaluationOutcome(
                    RuleEvaluationStatus.Error,
                    0,
                    null,
                    exception.Message);
            }

            evaluations.Add(
                new RuleEvaluationEntity
                {
                    Id = Guid.NewGuid(),

                    AuditId = audit.Id,
                    Audit = audit,

                    AuditRuleId = rule.Id,
                    AuditRule = rule,

                    Status = outcome.Status,

                    FindingCount =
                        outcome.FindingCount,

                    EvidenceJson =
                        outcome.EvidenceJson,

                    Recommendation =
                        rule.Recommendation,

                    ErrorMessage =
                        outcome.ErrorMessage,

                    EvaluatedAt =
                        evaluatedAt
                });
        }

        var compliantCount = evaluations.Count(
            evaluation =>
                evaluation.Status ==
                RuleEvaluationStatus.Compliant);

        var nonCompliantCount = evaluations.Count(
            evaluation =>
                evaluation.Status ==
                RuleEvaluationStatus.NonCompliant);

        var notApplicableCount = evaluations.Count(
            evaluation =>
                evaluation.Status ==
                RuleEvaluationStatus.NotApplicable);

        var notVerifiableCount = evaluations.Count(
            evaluation =>
                evaluation.Status ==
                RuleEvaluationStatus.NotVerifiable);

        var errorCount = evaluations.Count(
            evaluation =>
                evaluation.Status ==
                RuleEvaluationStatus.Error);

        var verifiableRuleCount =
            compliantCount + nonCompliantCount;

        decimal? complianceScore =
            verifiableRuleCount == 0
                ? null
                : Math.Round(
                    compliantCount * 100m /
                    verifiableRuleCount,
                    2,
                    MidpointRounding.AwayFromZero);

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        await _dbContext.RuleEvaluations
            .Where(evaluation =>
                evaluation.AuditId == auditId)
            .ExecuteDeleteAsync(
                cancellationToken);

        _dbContext.RuleEvaluations.AddRange(
            evaluations);

        audit.ComplianceScore =
            complianceScore;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        var evaluationDtos = evaluations
            .OrderBy(evaluation =>
                evaluation.AuditRule.Code)
            .Select(MapToDto)
            .ToArray();

        return new EvaluateAuditRulesResult
        {
            AuditId = auditId,

            EvaluatedRuleCount =
                evaluationDtos.Length,

            CompliantCount =
                compliantCount,

            NonCompliantCount =
                nonCompliantCount,

            NotApplicableCount =
                notApplicableCount,

            NotVerifiableCount =
                notVerifiableCount,

            ErrorCount =
                errorCount,

            ComplianceScore =
                complianceScore,

            Evaluations =
                evaluationDtos
        };
    }

    public async Task<
        IReadOnlyCollection<RuleEvaluationDto>>
        GetByAuditAsync(
            Guid auditId,
            CancellationToken cancellationToken = default)
    {
        var auditExists =
            await _dbContext.Audits
                .AnyAsync(
                    audit => audit.Id == auditId,
                    cancellationToken);

        if (!auditExists)
        {
            throw new KeyNotFoundException(
                $"L'audit {auditId} est introuvable.");
        }

        return await _dbContext.RuleEvaluations
            .AsNoTracking()
            .Include(evaluation =>
                evaluation.AuditRule)
            .Where(evaluation =>
                evaluation.AuditId == auditId)
            .OrderBy(evaluation =>
                evaluation.AuditRule.Code)
            .Select(evaluation =>
                new RuleEvaluationDto
                {
                    Id = evaluation.Id,

                    AuditId =
                        evaluation.AuditId,

                    AuditRuleId =
                        evaluation.AuditRuleId,

                    RuleCode =
                        evaluation.AuditRule.Code,

                    RuleName =
                        evaluation.AuditRule.Name,

                    CisControl =
                        evaluation.AuditRule.CisControl,

                    TargetType =
                        evaluation.AuditRule.TargetType,

                    Severity =
                        evaluation.AuditRule.Severity,

                    Status =
                        evaluation.Status,

                    FindingCount =
                        evaluation.FindingCount,

                    EvidenceJson =
                        evaluation.EvidenceJson,

                    Recommendation =
                        evaluation.Recommendation,

                    ErrorMessage =
                        evaluation.ErrorMessage,

                    EvaluatedAt =
                        evaluation.EvaluatedAt
                })
            .ToArrayAsync(cancellationToken);
    }

    private static EvaluationOutcome
        EvaluateInactiveAccounts(
            AuditEntity audit,
            DateTimeOffset referenceTime)
    {
        var threshold =
            referenceTime.AddDays(-90);

        var verifiableIdentities =
            audit.Identities
                .Where(identity =>
                    identity.IsEnabled &&
                    identity.LastSignInAt.HasValue)
                .ToArray();

        if (verifiableIdentities.Length == 0)
        {
            return new EvaluationOutcome(
                RuleEvaluationStatus.NotVerifiable,
                0,
                null,
                null);
        }

        var findings = verifiableIdentities
            .Where(identity =>
                identity.LastSignInAt!.Value <
                threshold)
            .Select(identity =>
                (object)new
                {
                    identity.Id,
                    identity.ExternalId,
                    identity.DisplayName,
                    identity.UserName,
                    identity.AccountType,
                    identity.LastSignInAt,
                    inactivityThreshold =
                        threshold,
                    reason =
                        "Compte actif sans connexion " +
                        "depuis plus de 90 jours"
                })
            .ToArray();

        return CreateFindingOutcome(findings);
    }

    private static EvaluationOutcome
        EvaluateDisabledPrivilegedAccounts(
            AuditEntity audit,
            DateTimeOffset referenceTime)
    {
        var findings =
            new List<object>();

        foreach (var identity in audit.Identities
                     .Where(identity =>
                         !identity.IsEnabled))
        {
            var privilegedRoles =
                identity.RoleAssignments
                    .Where(assignment =>
                        assignment.DirectoryRole
                            .IsPrivileged &&
                        IsAssignmentActive(
                            assignment.AssignedAt,
                            assignment.ExpiresAt,
                            referenceTime))
                    .Select(assignment =>
                        new
                        {
                            assignment.DirectoryRole
                                .ExternalId,

                            assignment.DirectoryRole
                                .Name,

                            assignment.IsPermanent,

                            assignment.AssignedAt,

                            assignment.ExpiresAt
                        })
                    .ToArray();

            var privilegedGroups =
                identity.GroupMemberships
                    .Where(membership =>
                        membership.Group
                            .IsPrivileged)
                    .Select(membership =>
                        new
                        {
                            membership.Group.ExternalId,
                            membership.Group.Name,
                            membership.MembershipType
                        })
                    .ToArray();

            if (privilegedRoles.Length == 0 &&
                privilegedGroups.Length == 0)
            {
                continue;
            }

            findings.Add(
                new
                {
                    identity.Id,
                    identity.ExternalId,
                    identity.DisplayName,
                    identity.UserName,
                    privilegedRoles,
                    privilegedGroups,
                    reason =
                        "Compte désactivé conservant " +
                        "des privilèges"
                });
        }

        return CreateFindingOutcome(findings);
    }

    private static EvaluationOutcome
        EvaluateServiceAccountsWithoutOwner(
            AuditEntity audit)
    {
        var serviceAccounts =
            audit.Identities
                .Where(identity =>
                    identity.IsServiceAccount ||
                    identity.AccountType ==
                    AccountType.ServiceAccount)
                .ToArray();

        if (serviceAccounts.Length == 0)
        {
            return new EvaluationOutcome(
                RuleEvaluationStatus.NotApplicable,
                0,
                null,
                null);
        }

        var findings = serviceAccounts
            .Where(identity =>
                string.IsNullOrWhiteSpace(
                    identity.Owner))
            .Select(identity =>
                (object)new
                {
                    identity.Id,
                    identity.ExternalId,
                    identity.DisplayName,
                    identity.UserName,
                    identity.Description,
                    identity.Owner,
                    reason =
                        "Compte de service sans " +
                        "propriétaire identifié"
                })
            .ToArray();

        return CreateFindingOutcome(findings);
    }

    private static EvaluationOutcome
        EvaluatePermanentPrivilegedAssignments(
            AuditEntity audit,
            DateTimeOffset referenceTime)
    {
        var findings =
            audit.Identities
                .SelectMany(identity =>
                    identity.RoleAssignments
                        .Where(assignment =>
                            assignment.IsPermanent &&
                            assignment.DirectoryRole
                                .IsPrivileged &&
                            IsAssignmentActive(
                                assignment.AssignedAt,
                                assignment.ExpiresAt,
                                referenceTime))
                        .Select(assignment =>
                            (object)new
                            {
                                identity.Id,
                                identity.ExternalId,
                                identity.DisplayName,
                                identity.UserName,

                                roleExternalId =
                                    assignment.DirectoryRole
                                        .ExternalId,

                                roleName =
                                    assignment.DirectoryRole
                                        .Name,

                                assignment.AssignedAt,
                                assignment.ExpiresAt,
                                assignment.IsPermanent,

                                reason =
                                    "Affectation permanente " +
                                    "à un rôle privilégié"
                            }))
                .ToArray();

        return CreateFindingOutcome(findings);
    }

    private static EvaluationOutcome
        EvaluatePrivilegedGroupMemberships(
            AuditEntity audit)
    {
        var findings =
            audit.Identities
                .SelectMany(identity =>
                    identity.GroupMemberships
                        .Where(membership =>
                            membership.Group
                                .IsPrivileged)
                        .Select(membership =>
                            (object)new
                            {
                                identity.Id,
                                identity.ExternalId,
                                identity.DisplayName,
                                identity.UserName,

                                groupExternalId =
                                    membership.Group
                                        .ExternalId,

                                groupName =
                                    membership.Group
                                        .Name,

                                membership
                                    .MembershipType,

                                reason =
                                    "Appartenance à un " +
                                    "groupe privilégié"
                            }))
                .ToArray();

        return CreateFindingOutcome(findings);
    }

    private static EvaluationOutcome
        EvaluatePrivilegedGuestAccounts(
            AuditEntity audit,
            DateTimeOffset referenceTime)
    {
        var guestAccounts =
            audit.Identities
                .Where(identity =>
                    identity.AccountType ==
                    AccountType.Guest)
                .ToArray();

        if (guestAccounts.Length == 0)
        {
            return new EvaluationOutcome(
                RuleEvaluationStatus.NotApplicable,
                0,
                null,
                null);
        }

        var findings =
            new List<object>();

        foreach (var identity in guestAccounts)
        {
            var privilegedRoles =
                identity.RoleAssignments
                    .Where(assignment =>
                        assignment.DirectoryRole
                            .IsPrivileged &&
                        IsAssignmentActive(
                            assignment.AssignedAt,
                            assignment.ExpiresAt,
                            referenceTime))
                    .Select(assignment =>
                        new
                        {
                            assignment.DirectoryRole
                                .ExternalId,

                            assignment.DirectoryRole
                                .Name,

                            assignment.IsPermanent,

                            assignment.AssignedAt,

                            assignment.ExpiresAt
                        })
                    .ToArray();

            var privilegedGroups =
                identity.GroupMemberships
                    .Where(membership =>
                        membership.Group
                            .IsPrivileged)
                    .Select(membership =>
                        new
                        {
                            membership.Group.ExternalId,
                            membership.Group.Name,
                            membership.MembershipType
                        })
                    .ToArray();

            if (privilegedRoles.Length == 0 &&
                privilegedGroups.Length == 0)
            {
                continue;
            }

            findings.Add(
                new
                {
                    identity.Id,
                    identity.ExternalId,
                    identity.DisplayName,
                    identity.UserName,
                    privilegedRoles,
                    privilegedGroups,
                    reason =
                        "Compte invité disposant " +
                        "de privilèges"
                });
        }

        return CreateFindingOutcome(findings);
    }

    private static bool IsAssignmentActive(
        DateTimeOffset? assignedAt,
        DateTimeOffset? expiresAt,
        DateTimeOffset referenceTime)
    {
        var hasStarted =
            !assignedAt.HasValue ||
            assignedAt.Value <= referenceTime;

        var hasNotExpired =
            !expiresAt.HasValue ||
            expiresAt.Value > referenceTime;

        return hasStarted && hasNotExpired;
    }

    private static EvaluationOutcome
        CreateFindingOutcome(
            IReadOnlyCollection<object> findings)
    {
        if (findings.Count == 0)
        {
            return new EvaluationOutcome(
                RuleEvaluationStatus.Compliant,
                0,
                null,
                null);
        }

        return new EvaluationOutcome(
            RuleEvaluationStatus.NonCompliant,
            findings.Count,
            JsonSerializer.Serialize(
                findings,
                JsonOptions),
            null);
    }

    private static RuleEvaluationDto MapToDto(
        RuleEvaluationEntity evaluation)
    {
        return new RuleEvaluationDto
        {
            Id = evaluation.Id,

            AuditId =
                evaluation.AuditId,

            AuditRuleId =
                evaluation.AuditRuleId,

            RuleCode =
                evaluation.AuditRule.Code,

            RuleName =
                evaluation.AuditRule.Name,

            CisControl =
                evaluation.AuditRule.CisControl,

            TargetType =
                evaluation.AuditRule.TargetType,

            Severity =
                evaluation.AuditRule.Severity,

            Status =
                evaluation.Status,

            FindingCount =
                evaluation.FindingCount,

            EvidenceJson =
                evaluation.EvidenceJson,

            Recommendation =
                evaluation.Recommendation,

            ErrorMessage =
                evaluation.ErrorMessage,

            EvaluatedAt =
                evaluation.EvaluatedAt
        };
    }

    private sealed record EvaluationOutcome(
        RuleEvaluationStatus Status,
        int FindingCount,
        string? EvidenceJson,
        string? ErrorMessage);
}