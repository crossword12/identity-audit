using Microsoft.EntityFrameworkCore;
using AuditEntity = IdentityAudit.Domain.Entities.Audit;
using TargetEntity = IdentityAudit.Domain.Entities.Target;
using DirectoryIdentityEntity = IdentityAudit.Domain.Entities.DirectoryIdentity;
using DirectoryGroupEntity = IdentityAudit.Domain.Entities.DirectoryGroup;
using GroupMembershipEntity = IdentityAudit.Domain.Entities.GroupMembership;

namespace IdentityAudit.Infrastructure.Persistence;

public sealed class IdentityAuditDbContext
    : DbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureTarget(modelBuilder);
        ConfigureAudit(modelBuilder);
        ConfigureDirectoryIdentity(modelBuilder);
        ConfigureDirectoryGroup(modelBuilder);
        ConfigureGroupMembership(modelBuilder);
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
}