using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.Targets;

public sealed class UpdateTargetRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; init; } = string.Empty;

    public bool IsEnabled { get; init; } = true;

    public string? ConfigurationJson { get; init; }
}