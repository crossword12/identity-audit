using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.GroupMemberships;

public sealed class ImportGroupMembershipsRequest
{
    [Required]
    public IReadOnlyCollection<CollectedGroupMembershipRequest>
        Memberships
    { get; init; }
        = Array.Empty<CollectedGroupMembershipRequest>();
}