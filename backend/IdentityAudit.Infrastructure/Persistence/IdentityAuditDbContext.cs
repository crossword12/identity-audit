using IdentityAudit.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AuditEntity = IdentityAudit.Domain.Entities.Audit;
using TargetEntity = IdentityAudit.Domain.Entities.Target;
using DirectoryIdentityEntity = IdentityAudit.Domain.Entities.DirectoryIdentity;
using DirectoryGroupEntity = IdentityAudit.Domain.Entities.DirectoryGroup;
using GroupMembershipEntity = IdentityAudit.Domain.Entities.GroupMembership;
using DirectoryRoleEntity =
    IdentityAudit.Domain.Entities.DirectoryRole;

using RoleAssignmentEntity =
    IdentityAudit.Domain.Entities.RoleAssignment;
using AuditRuleEntity =
    IdentityAudit.Domain.Entities.AuditRule;

using RuleEvaluationEntity =
    IdentityAudit.Domain.Entities.RuleEvaluation;

namespace IdentityAudit.Infrastructure.Persistence;

public sealed class IdentityAuditDbContext
    : IdentityDbContext<
        ApplicationUser,
        IdentityRole<Guid>,
        Guid>
{
    public IdentityAuditDbContext(
        DbContextOptions<IdentityAuditDbContext> options)
        : base(options)
    {
    }

    public DbSet<TargetEntity> Targets => Set<TargetEntity>();

    public DbSet<AuditEntity> Audits => Set<AuditEntity>();

    public DbSet<DirectoryIdentityEntity> Identities
    => Set<DirectoryIdentityEntity>();

    public DbSet<DirectoryGroupEntity> DirectoryGroups
    => Set<DirectoryGroupEntity>();

    public DbSet<GroupMembershipEntity> GroupMemberships
        => Set<GroupMembershipEntity>();

    public DbSet<DirectoryRoleEntity> DirectoryRoles =>
    Set<DirectoryRoleEntity>();

    public DbSet<RoleAssignmentEntity> RoleAssignments =>
        Set<RoleAssignmentEntity>();

    public DbSet<AuditRuleEntity> AuditRules =>
    Set<AuditRuleEntity>();

    public DbSet<RuleEvaluationEntity> RuleEvaluations =>
        Set<RuleEvaluationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureApplicationIdentity(modelBuilder);
        ConfigureTarget(modelBuilder);
        ConfigureAudit(modelBuilder);
        ConfigureDirectoryIdentity(modelBuilder);
        ConfigureDirectoryGroup(modelBuilder);
        ConfigureGroupMembership(modelBuilder);
        ConfigureAuditRule(modelBuilder);
        ConfigureRuleEvaluation(modelBuilder);

        modelBuilder.Entity<DirectoryRoleEntity>(entity =>
{
    entity.ToTable("DirectoryRoles");

    entity.HasKey(role => role.Id);

    entity.Property(role => role.ExternalId)
        .HasMaxLength(512)
        .IsRequired();

    entity.Property(role => role.Name)
        .HasMaxLength(255)
        .IsRequired();

    entity.Property(role => role.Description)
        .HasMaxLength(2000);

    entity.Property(role => role.Source)
        .HasConversion<string>()
        .HasMaxLength(30)
        .IsRequired();

    entity.Property(role => role.IsPrivileged)
        .IsRequired();

    entity.Property(role => role.CollectedAt)
        .IsRequired();

    entity.HasIndex(role => new
    {
        role.AuditId,
        role.ExternalId
    })
        .IsUnique();

    entity.HasOne(role => role.Audit)
        .WithMany(audit => audit.Roles)
        .HasForeignKey(role => role.AuditId)
        .OnDelete(DeleteBehavior.Cascade);
});

        modelBuilder.Entity<RoleAssignmentEntity>(entity =>
        {
            entity.ToTable("RoleAssignments");

            entity.HasKey(assignment => assignment.Id);

            entity.Property(assignment => assignment.AssignedAt);

            entity.Property(assignment => assignment.ExpiresAt);

            entity.Property(assignment => assignment.IsPermanent)
                .IsRequired();

            entity.Property(assignment => assignment.CollectedAt)
                .IsRequired();

            entity.HasIndex(assignment => new
            {
                assignment.IdentityId,
                assignment.DirectoryRoleId
            })
                .IsUnique();

            entity.HasOne(assignment => assignment.Identity)
                .WithMany(identity => identity.RoleAssignments)
                .HasForeignKey(assignment => assignment.IdentityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(assignment => assignment.DirectoryRole)
                .WithMany(role => role.Assignments)
                .HasForeignKey(assignment =>
                    assignment.DirectoryRoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTarget(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TargetEntity>(entity =>
        {
            entity.ToTable("Targets");

            entity.HasKey(target => target.Id);

            entity.Property(target => target.Name)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(target => target.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(target => target.IsEnabled)
                .IsRequired();

            entity.Property(target => target.ConfigurationJson)
                .HasColumnType("jsonb");

            entity.Property(target => target.CreatedAt)
                .IsRequired();

            entity.Property(target => target.UpdatedAt)
                .IsRequired();

            entity.HasMany(target => target.Audits)
                .WithOne(audit => audit.Target)
                .HasForeignKey(audit => audit.TargetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditEntity>(entity =>
        {
            entity.ToTable("Audits");

            entity.HasKey(audit => audit.Id);

            entity.Property(audit => audit.Status)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(audit => audit.ComplianceScore)
                .HasPrecision(5, 2);

            entity.Property(audit => audit.ErrorMessage)
                .HasColumnType("text");

            entity.Property(audit => audit.CreatedAt)
                .IsRequired();

            entity.HasIndex(audit => new
            {
                audit.TargetId,
                audit.CreatedAt
            })
                .IsDescending(false, true)
                .HasDatabaseName("IX_Audits_TargetId_CreatedAt");
        });
    }

    private static void ConfigureDirectoryIdentity(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DirectoryIdentityEntity>(entity =>
        {
            entity.ToTable("Identities");

            entity.HasKey(identity => identity.Id);

            entity.Property(identity => identity.ExternalId)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(identity => identity.DisplayName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(identity => identity.UserName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(identity => identity.Email)
                .HasMaxLength(320);

            entity.Property(identity => identity.Source)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(identity => identity.AccountType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(identity => identity.Description)
                .HasMaxLength(2000);

            entity.Property(identity => identity.Owner)
                .HasMaxLength(255);

            entity.Property(identity => identity.CollectedAt)
                .IsRequired();

            entity.HasIndex(identity => new
            {
                identity.AuditId,
                identity.ExternalId
            })
            .IsUnique();

            entity.HasOne(identity => identity.Audit)
                .WithMany(audit => audit.Identities)
                .HasForeignKey(identity => identity.AuditId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureDirectoryGroup(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DirectoryGroupEntity>(entity =>
        {
            entity.ToTable("DirectoryGroups");

            entity.HasKey(group => group.Id);

            entity.Property(group => group.ExternalId)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(group => group.Name)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(group => group.Description)
                .HasMaxLength(2000);

            entity.Property(group => group.Source)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(group => group.GroupType)
                .HasMaxLength(100);

            entity.Property(group => group.IsPrivileged)
                .IsRequired();

            entity.Property(group => group.CollectedAt)
                .IsRequired();

            entity.HasIndex(group => new
            {
                group.AuditId,
                group.ExternalId
            })
            .IsUnique()
            .HasDatabaseName(
                "IX_DirectoryGroups_AuditId_ExternalId");

            entity.HasOne(group => group.Audit)
                .WithMany(audit => audit.Groups)
                .HasForeignKey(group => group.AuditId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureGroupMembership(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GroupMembershipEntity>(entity =>
        {
            entity.ToTable("GroupMemberships");

            entity.HasKey(membership => membership.Id);

            entity.Property(membership =>
                    membership.MembershipType)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(membership =>
                    membership.CollectedAt)
                .IsRequired();

            entity.HasIndex(membership => new
            {
                membership.IdentityId,
                membership.GroupId
            })
            .IsUnique()
            .HasDatabaseName(
                "IX_GroupMemberships_IdentityId_GroupId");

            entity.HasOne(membership =>
                    membership.Identity)
                .WithMany(identity =>
                    identity.GroupMemberships)
                .HasForeignKey(membership =>
                    membership.IdentityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(membership =>
                    membership.Group)
                .WithMany(group =>
                    group.Memberships)
                .HasForeignKey(membership =>
                    membership.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAuditRule(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditRuleEntity>(entity =>
        {
            entity.ToTable("AuditRules");

            entity.HasKey(rule => rule.Id);

            entity.Property(rule => rule.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(rule => rule.Name)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(rule => rule.Description)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(rule => rule.CisControl)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(rule => rule.TargetType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(rule => rule.Severity)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(rule => rule.Recommendation)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(rule => rule.IsEnabled)
                .IsRequired();

            entity.Property(rule => rule.CreatedAt)
                .IsRequired();

            entity.Property(rule => rule.UpdatedAt)
                .IsRequired();

            entity.HasIndex(rule => rule.Code)
                .IsUnique()
                .HasDatabaseName(
                    "IX_AuditRules_Code");
        });
    }

    private static void ConfigureRuleEvaluation(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RuleEvaluationEntity>(entity =>
        {
            entity.ToTable("RuleEvaluations");

            entity.HasKey(evaluation => evaluation.Id);

            entity.Property(evaluation => evaluation.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(evaluation => evaluation.FindingCount)
                .IsRequired();

            entity.Property(evaluation => evaluation.EvidenceJson)
                .HasColumnType("jsonb");

            entity.Property(evaluation => evaluation.Recommendation)
                .HasMaxLength(2000);

            entity.Property(evaluation => evaluation.ErrorMessage)
                .HasColumnType("text");

            entity.Property(evaluation => evaluation.EvaluatedAt)
                .IsRequired();

            entity.HasIndex(evaluation => new
            {
                evaluation.AuditId,
                evaluation.AuditRuleId
            })
                .IsUnique()
                .HasDatabaseName(
                    "IX_RuleEvaluations_AuditId_AuditRuleId");

            entity.HasOne(evaluation => evaluation.Audit)
                .WithMany(audit => audit.RuleEvaluations)
                .HasForeignKey(evaluation => evaluation.AuditId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(evaluation => evaluation.AuditRule)
                .WithMany(rule => rule.Evaluations)
                .HasForeignKey(evaluation =>
                    evaluation.AuditRuleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    private static void ConfigureApplicationIdentity(
    ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("ApplicationUsers");

            entity.Property(user => user.DisplayName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(user => user.Email)
                .HasMaxLength(320);

            entity.Property(user => user.IsEnabled)
                .IsRequired();

            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.Property(user => user.UpdatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<IdentityRole<Guid>>()
            .ToTable("ApplicationRoles");

        modelBuilder.Entity<IdentityUserRole<Guid>>()
            .ToTable("ApplicationUserRoles");

        modelBuilder.Entity<IdentityUserClaim<Guid>>()
            .ToTable("ApplicationUserClaims");

        modelBuilder.Entity<IdentityUserLogin<Guid>>()
            .ToTable("ApplicationUserLogins");

        modelBuilder.Entity<IdentityRoleClaim<Guid>>()
            .ToTable("ApplicationRoleClaims");

        modelBuilder.Entity<IdentityUserToken<Guid>>()
            .ToTable("ApplicationUserTokens");
    }
}