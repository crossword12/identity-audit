using IdentityAudit.Domain.Entities;
using IdentityAudit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IdentityAudit.Infrastructure.Persistence.Seeding;

public static class AuditRuleCatalogSeeder
{
    public static async Task<int> SeedAsync(
        IdentityAuditDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var now = DateTimeOffset.UtcNow;

        var catalog = CreateCatalog(now);

        var catalogCodes = catalog
            .Select(rule => rule.Code)
            .ToArray();

        var existingCodes = await dbContext.AuditRules
            .AsNoTracking()
            .Where(rule => catalogCodes.Contains(rule.Code))
            .Select(rule => rule.Code)
            .ToListAsync(cancellationToken);

        var existingCodeSet = existingCodes.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        var missingRules = catalog
            .Where(rule =>
                !existingCodeSet.Contains(rule.Code))
            .ToList();

        if (missingRules.Count == 0)
        {
            return 0;
        }

        dbContext.AuditRules.AddRange(missingRules);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return missingRules.Count;
    }

    private static IReadOnlyCollection<AuditRule>
        CreateCatalog(DateTimeOffset now)
    {
        return
        [
            new AuditRule
            {
                Id = Guid.Parse(
                    "50000000-0000-0000-0000-000000000001"),

                Code = "ENTRA-05-01",

                Name =
                    "Comptes actifs inactifs depuis plus de 90 jours",

                Description =
                    "Détecte les comptes toujours actifs dont la " +
                    "dernière connexion remonte à plus de 90 jours.",

                CisControl = "CIS Control 5",

                TargetType = TargetType.EntraId,

                Severity = RuleSeverity.Medium,

                Recommendation =
                    "Examiner l'utilisation et le propriétaire du " +
                    "compte, puis désactiver ou supprimer les comptes " +
                    "qui ne sont plus nécessaires.",

                IsEnabled = true,

                CreatedAt = now,
                UpdatedAt = now
            },

            new AuditRule
            {
                Id = Guid.Parse(
                    "50000000-0000-0000-0000-000000000002"),

                Code = "ENTRA-05-02",

                Name =
                    "Comptes désactivés conservant des privilèges",

                Description =
                    "Détecte les comptes désactivés qui conservent " +
                    "une affectation active à un rôle privilégié ou " +
                    "une appartenance à un groupe privilégié.",

                CisControl = "CIS Control 5",

                TargetType = TargetType.EntraId,

                Severity = RuleSeverity.High,

                Recommendation =
                    "Retirer les rôles et appartenances privilégiés " +
                    "des comptes désactivés, puis vérifier que ces " +
                    "comptes ne sont plus utilisés.",

                IsEnabled = true,

                CreatedAt = now,
                UpdatedAt = now
            },

            new AuditRule
            {
                Id = Guid.Parse(
                    "50000000-0000-0000-0000-000000000003"),

                Code = "ENTRA-05-03",

                Name =
                    "Comptes de service sans propriétaire identifié",

                Description =
                    "Détecte les comptes de service pour lesquels " +
                    "aucun propriétaire responsable n'est renseigné.",

                CisControl = "CIS Control 5",

                TargetType = TargetType.EntraId,

                Severity = RuleSeverity.Medium,

                Recommendation =
                    "Attribuer un propriétaire à chaque compte de " +
                    "service et documenter son usage, sa criticité " +
                    "et sa procédure de révision.",

                IsEnabled = true,

                CreatedAt = now,
                UpdatedAt = now
            },

            new AuditRule
            {
                Id = Guid.Parse(
                    "60000000-0000-0000-0000-000000000001"),

                Code = "ENTRA-06-01",

                Name =
                    "Affectations permanentes de rôles privilégiés",

                Description =
                    "Détecte les identités disposant d'une " +
                    "affectation permanente à un rôle considéré " +
                    "comme privilégié.",

                CisControl = "CIS Control 6",

                TargetType = TargetType.EntraId,

                Severity = RuleSeverity.High,

                Recommendation =
                    "Remplacer les affectations permanentes par des " +
                    "accès temporaires, approuvés et régulièrement " +
                    "réévalués lorsque cela est possible.",

                IsEnabled = true,

                CreatedAt = now,
                UpdatedAt = now
            },

            new AuditRule
            {
                Id = Guid.Parse(
                    "60000000-0000-0000-0000-000000000002"),

                Code = "ENTRA-06-02",

                Name =
                    "Appartenances aux groupes privilégiés",

                Description =
                    "Identifie les comptes appartenant à des groupes " +
                    "marqués comme privilégiés afin de permettre une " +
                    "révision des droits accordés.",

                CisControl = "CIS Control 6",

                TargetType = TargetType.EntraId,

                Severity = RuleSeverity.Medium,

                Recommendation =
                    "Vérifier que chaque appartenance à un groupe " +
                    "privilégié est justifiée, approuvée et encore " +
                    "nécessaire.",

                IsEnabled = true,

                CreatedAt = now,
                UpdatedAt = now
            },

            new AuditRule
            {
                Id = Guid.Parse(
                    "60000000-0000-0000-0000-000000000003"),

                Code = "ENTRA-06-03",

                Name =
                    "Comptes invités disposant de privilèges",

                Description =
                    "Détecte les comptes invités qui bénéficient " +
                    "d'un rôle privilégié ou appartiennent à un " +
                    "groupe privilégié.",

                CisControl = "CIS Control 6",

                TargetType = TargetType.EntraId,

                Severity = RuleSeverity.High,

                Recommendation =
                    "Supprimer les privilèges non indispensables des " +
                    "comptes invités et limiter leur accès dans le " +
                    "temps selon le principe du moindre privilège.",

                IsEnabled = true,

                CreatedAt = now,
                UpdatedAt = now
            },

                        new AuditRule
            {
                Id = Guid.Parse(
                    "51000000-0000-0000-0000-000000000001"),

                Code = "AD-05-01",

                Name =
                    "Comptes actifs inactifs depuis plus de 90 jours",

                Description =
                    "Détecte les comptes Active Directory toujours " +
                    "actifs dont la dernière activité connue remonte " +
                    "à plus de 90 jours.",

                CisControl = "CIS Control 5",

                TargetType =
                    TargetType.ActiveDirectory,

                Severity =
                    RuleSeverity.Medium,

                Recommendation =
                    "Examiner l'utilisation du compte et confirmer " +
                    "qu'il est toujours nécessaire. Désactiver ou " +
                    "supprimer les comptes devenus inutiles.",

                IsEnabled = true,

                CreatedAt = now,
                UpdatedAt = now
            },

            new AuditRule
            {
                Id = Guid.Parse(
                    "51000000-0000-0000-0000-000000000002"),

                Code = "AD-05-02",

                Name =
                    "Comptes désactivés conservant des privilèges",

                Description =
                    "Détecte les comptes Active Directory désactivés " +
                    "qui conservent une appartenance directe ou " +
                    "transitive à un groupe privilégié.",

                CisControl = "CIS Control 5",

                TargetType =
                    TargetType.ActiveDirectory,

                Severity =
                    RuleSeverity.High,

                Recommendation =
                    "Retirer les appartenances privilégiées des " +
                    "comptes désactivés et vérifier qu'aucun droit " +
                    "administratif résiduel n'est conservé.",

                IsEnabled = true,

                CreatedAt = now,
                UpdatedAt = now
            },

            new AuditRule
            {
                Id = Guid.Parse(
                    "51000000-0000-0000-0000-000000000003"),

                Code = "AD-05-03",

                Name =
                    "Comptes de service sans propriétaire identifié",

                Description =
                    "Détecte les comptes de service Active Directory " +
                    "pour lesquels aucun propriétaire responsable " +
                    "n'est renseigné.",

                CisControl = "CIS Control 5",

                TargetType =
                    TargetType.ActiveDirectory,

                Severity =
                    RuleSeverity.Medium,

                Recommendation =
                    "Attribuer un propriétaire responsable à chaque " +
                    "compte de service et documenter son usage, sa " +
                    "criticité et sa procédure de révision.",

                IsEnabled = true,

                CreatedAt = now,
                UpdatedAt = now
            },

            new AuditRule
            {
                Id = Guid.Parse(
                    "61000000-0000-0000-0000-000000000001"),

                Code = "AD-06-01",

                Name =
                    "Appartenances aux groupes privilégiés",

                Description =
                    "Identifie les comptes Active Directory disposant " +
                    "d'une appartenance directe ou transitive à un " +
                    "groupe considéré comme privilégié.",

                CisControl = "CIS Control 6",

                TargetType =
                    TargetType.ActiveDirectory,

                Severity =
                    RuleSeverity.High,

                Recommendation =
                    "Vérifier que chaque appartenance à un groupe " +
                    "privilégié est justifiée, approuvée et encore " +
                    "nécessaire selon le principe du moindre privilège.",

                IsEnabled = true,

                CreatedAt = now,
                UpdatedAt = now
            }
        ];
    }
}