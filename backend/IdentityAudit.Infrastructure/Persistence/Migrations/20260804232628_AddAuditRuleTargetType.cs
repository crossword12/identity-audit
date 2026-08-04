using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityAudit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditRuleTargetType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TargetType",
                table: "AuditRules",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "AuditRules");
        }
    }
}
