using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityAudit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichDashboardWithCisMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP FUNCTION IF EXISTS
                    public."fn_GetTargetDashboard"(uuid);
                """);

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW
                    public."vw_AuditDashboardSummary" AS

                WITH identity_summary AS
                (
                    SELECT
                        i."AuditId" AS "AuditId",

                        COUNT(i."Id")
                            AS "TotalIdentities",

                        COUNT(i."Id")
                            FILTER (
                                WHERE i."IsEnabled" = TRUE
                            )
                            AS "EnabledIdentities",

                        COUNT(i."Id")
                            FILTER (
                                WHERE i."IsEnabled" = FALSE
                            )
                            AS "DisabledIdentities",

                        COUNT(i."Id")
                            FILTER (
                                WHERE i."IsPrivileged" = TRUE
                            )
                            AS "PrivilegedIdentities",

                        COUNT(i."Id")
                            FILTER (
                                WHERE i."IsServiceAccount" = TRUE
                            )
                            AS "ServiceAccounts",

                        COUNT(i."Id")
                            FILTER (
                                WHERE i."AccountType" = 'Guest'
                            )
                            AS "GuestAccounts",

                        COUNT(i."Id")
                            FILTER (
                                WHERE i."IsLocked" = TRUE
                            )
                            AS "LockedAccounts"

                    FROM public."Identities" AS i

                    GROUP BY i."AuditId"
                ),

                evaluation_summary AS
                (
                    SELECT
                        e."AuditId" AS "AuditId",

                        COUNT(e."Id")
                            AS "EvaluatedRules",

                        COUNT(e."Id")
                            FILTER (
                                WHERE e."Status" = 'Compliant'
                            )
                            AS "CompliantRules",

                        COUNT(e."Id")
                            FILTER (
                                WHERE e."Status" = 'NonCompliant'
                            )
                            AS "NonCompliantRules",

                        COUNT(e."Id")
                            FILTER (
                                WHERE e."Status" = 'NotApplicable'
                            )
                            AS "NotApplicableRules",

                        COUNT(e."Id")
                            FILTER (
                                WHERE e."Status" = 'NotVerifiable'
                            )
                            AS "NotVerifiableRules",

                        COUNT(e."Id")
                            FILTER (
                                WHERE e."Status" = 'Error'
                            )
                            AS "ErrorRules",

                        COALESCE(
                            SUM(e."FindingCount"),
                            0
                        )::bigint
                            AS "TotalFindings",

                        COALESCE(
                            SUM(e."FindingCount")
                                FILTER (
                                    WHERE r."Severity" = 'Critical'
                                ),
                            0
                        )::bigint
                            AS "CriticalFindings",

                        COALESCE(
                            SUM(e."FindingCount")
                                FILTER (
                                    WHERE r."Severity" = 'High'
                                ),
                            0
                        )::bigint
                            AS "HighFindings",

                        COALESCE(
                            SUM(e."FindingCount")
                                FILTER (
                                    WHERE r."Severity" = 'Medium'
                                ),
                            0
                        )::bigint
                            AS "MediumFindings",

                        COALESCE(
                            SUM(e."FindingCount")
                                FILTER (
                                    WHERE r."Severity" = 'Low'
                                ),
                            0
                        )::bigint
                            AS "LowFindings",

                        COALESCE(
                            SUM(e."FindingCount")
                                FILTER (
                                    WHERE r."CisControl" = 'CIS Control 5'
                                ),
                            0
                        )::bigint
                            AS "CisControl5Findings",

                        COALESCE(
                            SUM(e."FindingCount")
                                FILTER (
                                    WHERE r."CisControl" = 'CIS Control 6'
                                ),
                            0
                        )::bigint
                            AS "CisControl6Findings"

                    FROM public."RuleEvaluations" AS e

                    INNER JOIN public."AuditRules" AS r
                        ON r."Id" = e."AuditRuleId"

                    GROUP BY e."AuditId"
                )

                SELECT
                    a."Id" AS "AuditId",
                    a."TargetId" AS "TargetId",
                    t."Name" AS "TargetName",
                    t."Type" AS "TargetType",
                    a."Status" AS "AuditStatus",
                    a."CreatedAt" AS "CreatedAt",
                    a."StartedAt" AS "StartedAt",
                    a."CompletedAt" AS "CompletedAt",
                    a."ComplianceScore" AS "ComplianceScore",

                    COALESCE(
                        identities."TotalIdentities",
                        0::bigint
                    ) AS "TotalIdentities",

                    COALESCE(
                        identities."EnabledIdentities",
                        0::bigint
                    ) AS "EnabledIdentities",

                    COALESCE(
                        identities."DisabledIdentities",
                        0::bigint
                    ) AS "DisabledIdentities",

                    COALESCE(
                        identities."PrivilegedIdentities",
                        0::bigint
                    ) AS "PrivilegedIdentities",

                    COALESCE(
                        identities."ServiceAccounts",
                        0::bigint
                    ) AS "ServiceAccounts",

                    COALESCE(
                        identities."GuestAccounts",
                        0::bigint
                    ) AS "GuestAccounts",

                    COALESCE(
                        identities."LockedAccounts",
                        0::bigint
                    ) AS "LockedAccounts",

                    COALESCE(
                        evaluations."EvaluatedRules",
                        0::bigint
                    ) AS "EvaluatedRules",

                    COALESCE(
                        evaluations."CompliantRules",
                        0::bigint
                    ) AS "CompliantRules",

                    COALESCE(
                        evaluations."NonCompliantRules",
                        0::bigint
                    ) AS "NonCompliantRules",

                    COALESCE(
                        evaluations."NotApplicableRules",
                        0::bigint
                    ) AS "NotApplicableRules",

                    COALESCE(
                        evaluations."NotVerifiableRules",
                        0::bigint
                    ) AS "NotVerifiableRules",

                    COALESCE(
                        evaluations."ErrorRules",
                        0::bigint
                    ) AS "ErrorRules",

                    COALESCE(
                        evaluations."TotalFindings",
                        0::bigint
                    ) AS "TotalFindings",

                    COALESCE(
                        evaluations."CriticalFindings",
                        0::bigint
                    ) AS "CriticalFindings",

                    COALESCE(
                        evaluations."HighFindings",
                        0::bigint
                    ) AS "HighFindings",

                    COALESCE(
                        evaluations."MediumFindings",
                        0::bigint
                    ) AS "MediumFindings",

                    COALESCE(
                        evaluations."LowFindings",
                        0::bigint
                    ) AS "LowFindings",

                    COALESCE(
                        evaluations."CisControl5Findings",
                        0::bigint
                    ) AS "CisControl5Findings",

                    COALESCE(
                        evaluations."CisControl6Findings",
                        0::bigint
                    ) AS "CisControl6Findings"

                FROM public."Audits" AS a

                INNER JOIN public."Targets" AS t
                    ON t."Id" = a."TargetId"

                LEFT JOIN identity_summary AS identities
                    ON identities."AuditId" = a."Id"

                LEFT JOIN evaluation_summary AS evaluations
                    ON evaluations."AuditId" = a."Id";
                """);

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION
                    public."fn_GetTargetDashboard"(
                        p_target_id uuid
                    )
                RETURNS SETOF
                    public."vw_AuditDashboardSummary"
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT *
                    FROM public."vw_AuditDashboardSummary"
                    WHERE "TargetId" = p_target_id
                    ORDER BY "CreatedAt" DESC;
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP FUNCTION IF EXISTS
                    public."fn_GetTargetDashboard"(uuid);
                """);

            migrationBuilder.Sql(
                """
                DROP VIEW IF EXISTS
                    public."vw_AuditDashboardSummary";
                """);

            migrationBuilder.Sql(
                """
                CREATE VIEW public."vw_AuditDashboardSummary" AS
                SELECT
                    a."Id" AS "AuditId",
                    a."TargetId" AS "TargetId",
                    t."Name" AS "TargetName",
                    t."Type" AS "TargetType",
                    a."Status" AS "AuditStatus",
                    a."CreatedAt" AS "CreatedAt",
                    a."StartedAt" AS "StartedAt",
                    a."CompletedAt" AS "CompletedAt",
                    a."ComplianceScore" AS "ComplianceScore",

                    COUNT(i."Id")
                        AS "TotalIdentities",

                    COUNT(i."Id")
                        FILTER (
                            WHERE i."IsEnabled" = TRUE
                        )
                        AS "EnabledIdentities",

                    COUNT(i."Id")
                        FILTER (
                            WHERE i."IsEnabled" = FALSE
                        )
                        AS "DisabledIdentities",

                    COUNT(i."Id")
                        FILTER (
                            WHERE i."IsPrivileged" = TRUE
                        )
                        AS "PrivilegedIdentities",

                    COUNT(i."Id")
                        FILTER (
                            WHERE i."IsServiceAccount" = TRUE
                        )
                        AS "ServiceAccounts",

                    COUNT(i."Id")
                        FILTER (
                            WHERE i."AccountType" = 'Guest'
                        )
                        AS "GuestAccounts",

                    COUNT(i."Id")
                        FILTER (
                            WHERE i."IsLocked" = TRUE
                        )
                        AS "LockedAccounts"

                FROM public."Audits" AS a

                INNER JOIN public."Targets" AS t
                    ON t."Id" = a."TargetId"

                LEFT JOIN public."Identities" AS i
                    ON i."AuditId" = a."Id"

                GROUP BY
                    a."Id",
                    a."TargetId",
                    t."Id",
                    t."Name",
                    t."Type",
                    a."Status",
                    a."CreatedAt",
                    a."StartedAt",
                    a."CompletedAt",
                    a."ComplianceScore";
                """);

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION
                    public."fn_GetTargetDashboard"(
                        p_target_id uuid
                    )
                RETURNS SETOF
                    public."vw_AuditDashboardSummary"
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT *
                    FROM public."vw_AuditDashboardSummary"
                    WHERE "TargetId" = p_target_id
                    ORDER BY "CreatedAt" DESC;
                $$;
                """);
        }
    }
}
