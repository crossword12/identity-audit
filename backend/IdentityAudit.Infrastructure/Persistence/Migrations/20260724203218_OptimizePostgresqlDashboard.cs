using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityAudit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizePostgresqlDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Identities_AuditId",
                table: "Identities");

            migrationBuilder.DropIndex(
                name: "IX_Audits_TargetId",
                table: "Audits");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_TargetId_CreatedAt",
                table: "Audits",
                columns: new[] { "TargetId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW public."vw_AuditDashboardSummary" AS
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

                    COUNT(i."Id") AS "TotalIdentities",

                    COUNT(i."Id")
                        FILTER (WHERE i."IsEnabled" = TRUE)
                        AS "EnabledIdentities",

                    COUNT(i."Id")
                        FILTER (WHERE i."IsEnabled" = FALSE)
                        AS "DisabledIdentities",

                    COUNT(i."Id")
                        FILTER (WHERE i."IsPrivileged" = TRUE)
                        AS "PrivilegedIdentities",

                    COUNT(i."Id")
                        FILTER (WHERE i."IsServiceAccount" = TRUE)
                        AS "ServiceAccounts",

                    COUNT(i."Id")
                        FILTER (WHERE i."AccountType" = 'Guest')
                        AS "GuestAccounts",

                    COUNT(i."Id")
                        FILTER (WHERE i."IsLocked" = TRUE)
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
                CREATE OR REPLACE FUNCTION public."fn_GetTargetDashboard"(
                    p_target_id uuid
                )
                RETURNS SETOF public."vw_AuditDashboardSummary"
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

            migrationBuilder.DropIndex(
                name: "IX_Audits_TargetId_CreatedAt",
                table: "Audits");

            migrationBuilder.CreateIndex(
                name: "IX_Identities_AuditId",
                table: "Identities",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_TargetId",
                table: "Audits",
                column: "TargetId");
        }
    }
}
