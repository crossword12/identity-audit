using IdentityAudit.ActiveDirectoryCollector.Models;

namespace IdentityAudit.ActiveDirectoryCollector.Services;

public sealed class ActiveDirectoryGroupMapper
{
    private const int SecurityEnabledFlag =
        unchecked((int)0x80000000);

    private static readonly HashSet<uint>
        PrivilegedRids =
        [
            512, // Domain Admins
            518, // Schema Admins
            519, // Enterprise Admins
            520, // Group Policy Creator Owners

            544, // Administrators
            548, // Account Operators
            549, // Server Operators
            550, // Print Operators
            551  // Backup Operators
        ];

    public IReadOnlyCollection<CollectedGroupPayload> Map(
        IEnumerable<ActiveDirectoryGroup> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        return groups
            .Select(MapGroup)
            .ToList();
    }

    private static CollectedGroupPayload MapGroup(
        ActiveDirectoryGroup group)
    {
        var isSecurityGroup =
            (group.GroupTypeValue &
             SecurityEnabledFlag) != 0;

        return new CollectedGroupPayload
        {
            ExternalId =
                group.ObjectGuid.Trim(),

            Name =
                string.IsNullOrWhiteSpace(
                    group.Name)
                    ? group.SamAccountName.Trim()
                    : group.Name.Trim(),

            Description =
                NormalizeOptionalText(
                    group.Description),

            Source =
                "ActiveDirectory",

            GroupType =
                isSecurityGroup
                    ? "Security"
                    : "Distribution",

            IsPrivileged =
                IsPrivilegedGroup(
                    group.ObjectSid)
        };
    }

    private static bool IsPrivilegedGroup(
        string objectSid)
    {
        if (string.IsNullOrWhiteSpace(
            objectSid))
        {
            return false;
        }

        var lastSeparator =
            objectSid.LastIndexOf('-');

        if (lastSeparator < 0 ||
            lastSeparator ==
            objectSid.Length - 1)
        {
            return false;
        }

        var ridText =
            objectSid[
                (lastSeparator + 1)..];

        return uint.TryParse(
                   ridText,
                   out var rid)
               &&
               PrivilegedRids.Contains(
                   rid);
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
            ? null
            : value.Trim();
    }
}