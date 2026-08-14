namespace IdentityAudit.ActiveDirectoryCollector.Models;

public sealed class CollectedGroupMembershipPayload
{
    public string IdentityExternalId { get; init; }
        = string.Empty;

    public string GroupExternalId { get; init; }
        = string.Empty;

    public string MembershipType { get; init; }
        = "Direct";
}