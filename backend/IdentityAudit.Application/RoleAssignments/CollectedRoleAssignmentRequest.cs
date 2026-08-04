using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.RoleAssignments;

public sealed class CollectedRoleAssignmentRequest
{
    [Required]
    [MaxLength(512)]
    public string IdentityExternalId { get; init; }
        = string.Empty;

    [Required]
    [MaxLength(512)]
    public string RoleExternalId { get; init; }
        = string.Empty;

    public DateTimeOffset? AssignedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public bool IsPermanent { get; init; }
}