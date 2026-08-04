using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityAudit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDirectoryGroupsAndMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DirectoryGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GroupType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPrivileged = table.Column<bool>(type: "boolean", nullable: false),
                    CollectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectoryGroups_Audits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "Audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CollectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMemberships_DirectoryGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "DirectoryGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMemberships_Identities_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "Identities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryGroups_AuditId_ExternalId",
                table: "DirectoryGroups",
                columns: new[] { "AuditId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_GroupId",
                table: "GroupMemberships",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_IdentityId_GroupId",
                table: "GroupMemberships",
                columns: new[] { "IdentityId", "GroupId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupMemberships");

            migrationBuilder.DropTable(
                name: "DirectoryGroups");
        }
    }
}
