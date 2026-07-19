using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Application.Identities;

public sealed class DirectoryIdentityDto
{
    public Guid Id { get; init; }

    public Guid AuditId { get; init; }

    public string ExternalId { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string? Email { get; init; }

    public TargetType Source { get; init; }

    public AccountType AccountType { get; init; }

    public bool IsEnabled { get; init; }

    public bool IsPrivileged { get; init; }

    public bool IsServiceAccount { get; init; }

    public bool? IsLocked { get; init; }

    public DateTimeOffset? LastSignInAt { get; init; }

    public string? Description { get; init; }

    public string? Owner { get; init; }

    public DateTimeOffset CollectedAt { get; init; }
}