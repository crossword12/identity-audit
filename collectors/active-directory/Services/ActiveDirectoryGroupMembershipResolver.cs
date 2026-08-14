using IdentityAudit.ActiveDirectoryCollector.Models;

namespace IdentityAudit.ActiveDirectoryCollector.Services;

public sealed class ActiveDirectoryGroupMembershipResolver
{
    public IReadOnlyCollection<CollectedGroupMembershipPayload>
        Resolve(
            IEnumerable<ActiveDirectoryUser> users,
            IEnumerable<ActiveDirectoryGroup> groups)
    {
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(groups);

        var groupList =
            groups.ToList();

        var groupsByDistinguishedName =
            groupList
                .Where(group =>
                    !string.IsNullOrWhiteSpace(
                        group.DistinguishedName))
                .ToDictionary(
                    group =>
                        group.DistinguishedName.Trim(),
                    group => group,
                    StringComparer.OrdinalIgnoreCase);

        var result =
            new List<CollectedGroupMembershipPayload>();

        foreach (var user in users)
        {
            ResolveUserMemberships(
                user,
                groupsByDistinguishedName,
                result);
        }

        return result;
    }

    private static void ResolveUserMemberships(
        ActiveDirectoryUser user,
        IReadOnlyDictionary<
            string,
            ActiveDirectoryGroup> groupsByDistinguishedName,
        ICollection<CollectedGroupMembershipPayload> result)
    {
        var memberships =
            new Dictionary<
                string,
                string>(
                    StringComparer.OrdinalIgnoreCase);

        var queue =
            new Queue<string>();

        foreach (var directGroupDn
            in user.MemberOfDistinguishedNames)
        {
            if (!groupsByDistinguishedName.TryGetValue(
                    directGroupDn,
                    out var directGroup))
            {
                continue;
            }

            memberships[
                directGroup.ObjectGuid] =
                "Direct";

            queue.Enqueue(
                directGroup.DistinguishedName);
        }

        var visitedGroups =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        while (queue.Count > 0)
        {
            var currentGroupDn =
                queue.Dequeue();

            if (!visitedGroups.Add(
                    currentGroupDn))
            {
                continue;
            }

            if (!groupsByDistinguishedName.TryGetValue(
                    currentGroupDn,
                    out var currentGroup))
            {
                continue;
            }

            foreach (var parentGroupDn
                in currentGroup.MemberOfDistinguishedNames)
            {
                if (!groupsByDistinguishedName.TryGetValue(
                        parentGroupDn,
                        out var parentGroup))
                {
                    continue;
                }

                if (!memberships.ContainsKey(
                        parentGroup.ObjectGuid))
                {
                    memberships[
                        parentGroup.ObjectGuid] =
                        "Transitive";
                }

                queue.Enqueue(
                    parentGroup.DistinguishedName);
            }
        }

        foreach (var membership in memberships)
        {
            result.Add(
                new CollectedGroupMembershipPayload
                {
                    IdentityExternalId =
                        user.ObjectGuid,

                    GroupExternalId =
                        membership.Key,

                    MembershipType =
                        membership.Value
                });
        }
    }
}