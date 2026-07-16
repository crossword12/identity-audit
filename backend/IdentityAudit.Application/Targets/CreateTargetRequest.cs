using System.ComponentModel.DataAnnotations;
using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Application.Targets;

public sealed class CreateTargetRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; init; } = string.Empty;

    [EnumDataType(typeof(TargetType))]
    public TargetType Type { get; init; }

    public bool IsEnabled { get; init; } = true;

    public string? ConfigurationJson { get; init; }
}