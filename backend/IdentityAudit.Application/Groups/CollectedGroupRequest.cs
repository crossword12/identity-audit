using System.ComponentModel.DataAnnotations;
using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Application.Groups;

public sealed class CollectedGroupRequest
{
    [Required]
    [MaxLength(512)]
    public string ExternalId { get; init; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }

    [EnumDataType(typeof(TargetType))]
    public TargetType Source { get; init; }

    [MaxLength(100)]
    public string? GroupType { get; init; }

    public bool IsPrivileged { get; init; }
}