using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Domain.Entities;

public sealed class Target
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public TargetType Type { get; set; }

    public bool IsEnabled { get; set; } = true;

    public string? ConfigurationJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastCollectedAt { get; set; }

    public ICollection<Audit> Audits { get; set; } = new List<Audit>();
}