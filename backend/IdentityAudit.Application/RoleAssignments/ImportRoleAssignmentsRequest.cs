using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.RoleAssignments;

public sealed class ImportRoleAssignmentsRequest
{
    [Required]
    public IReadOnlyCollection<CollectedRoleAssignmentRequest>
        Assignments
    { get; init; }
        = Array.Empty<CollectedRoleAssignmentRequest>();
}