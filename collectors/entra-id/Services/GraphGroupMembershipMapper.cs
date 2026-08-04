using IdentityAudit.EntraCollector.Models;

namespace IdentityAudit.EntraCollector.Services;

public sealed class GraphGroupMembershipMapper
{
    public IReadOnlyCollection<CollectedGroupMembershipPayload> Map(
        GraphGroup group,
        IEnumerable<GraphGroupMember> members,
        IReadOnlySet<string> knownUserExternalIds)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(members);
        ArgumentNullException.ThrowIfNull(
            knownUserExternalIds);

        return members
            .Where(IsUser)
            .Where(member =>
                !string.IsNullOrWhiteSpace(member.Id))
            .Select(member => member.Id.Trim())
            .Where(knownUserExternalIds.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(identityExternalId =>
                new CollectedGroupMembershipPayload
                {
                    IdentityExternalId =
                        identityExternalId,

                    GroupExternalId =
                        group.Id.Trim(),

                    MembershipType =
                        "Direct"
                })
            .ToList();
    }

    private static bool IsUser(
        GraphGroupMember member)
    {
        return member.ODataType.Equals(
            "#microsoft.graph.user",
            StringComparison.OrdinalIgnoreCase);
    }
}