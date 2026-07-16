using Microsoft.EntityFrameworkCore;
using AuditEntity = IdentityAudit.Domain.Entities.Audit;
using TargetEntity = IdentityAudit.Domain.Entities.Target;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureTarget(modelBuilder);
        ConfigureAudit(modelBuilder);
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

            entity.HasIndex(audit => audit.TargetId);
        });
    }
}