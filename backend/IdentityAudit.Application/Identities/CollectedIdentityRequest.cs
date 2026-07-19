using System.ComponentModel.DataAnnotations;
using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Application.Identities;

public sealed class CollectedIdentityRequest
{
    [Required]
    [MaxLength(512)]
    public string ExternalId { get; init; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string UserName { get; init; } = string.Empty;

    [EmailAddress]
    [MaxLength(320)]
    public string? Email { get; init; }

    [EnumDataType(typeof(TargetType))]
    public TargetType Source { get; init; }

    [EnumDataType(typeof(AccountType))]
    public AccountType AccountType { get; init; }

    public bool IsEnabled { get; init; } = true;

    public bool IsPrivileged { get; init; }

    public bool IsServiceAccount { get; init; }

    public bool? IsLocked { get; init; }

    public DateTimeOffset? LastSignInAt { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }

    [MaxLength(255)]
    public string? Owner { get; init; }
}