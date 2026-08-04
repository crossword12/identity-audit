using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.GroupMemberships;

public sealed class CollectedGroupMembershipRequest
{
    [Required]
    [MaxLength(512)]
    public string IdentityExternalId { get; init; }
        = string.Empty;

    [Required]
    [MaxLength(512)]
    public string GroupExternalId { get; init; }
        = string.Empty;

    [Required]
    [MaxLength(30)]
    public string MembershipType { get; init; }
        = "Direct";
}