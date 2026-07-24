using Microsoft.EntityFrameworkCore;
using AuditEntity = IdentityAudit.Domain.Entities.Audit;
using TargetEntity = IdentityAudit.Domain.Entities.Target;
using DirectoryIdentityEntity = IdentityAudit.Domain.Entities.DirectoryIdentity;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureTarget(modelBuilder);
        ConfigureAudit(modelBuilder);
        ConfigureDirectoryIdentity(modelBuilder);
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
}