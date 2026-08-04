using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.Roles;

public sealed class CollectedRoleRequest
{
    [Required]
    [MaxLength(512)]
    public string ExternalId { get; init; }
        = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Name { get; init; }
        = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Required]
    [MaxLength(30)]
    public string Source { get; init; }
        = string.Empty;

    public bool IsPrivileged { get; init; }
}