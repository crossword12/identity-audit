using IdentityAudit.ActiveDirectoryCollector.Models;

namespace IdentityAudit.ActiveDirectoryCollector.Services;

public sealed class ActiveDirectoryUserMapper
{
    private const int AccountDisabledFlag = 0x0002;

    private const int AccountLockedFlag = 0x0010;

    public IReadOnlyCollection<CollectedIdentityPayload> Map(
        IEnumerable<ActiveDirectoryUser> users)
    {
        ArgumentNullException.ThrowIfNull(users);

        return users
            .Select(MapUser)
            .ToList();
    }

    private static CollectedIdentityPayload MapUser(
        ActiveDirectoryUser user)
    {
        var isEnabled =
            (user.UserAccountControl &
             AccountDisabledFlag) == 0;

        bool? isLocked =
            user.ComputedUserAccountControl.HasValue
                ? (user.ComputedUserAccountControl.Value &
                    AccountLockedFlag) != 0
                : null;

        var isServiceAccount =
            IsServiceAccount(user);

        return new CollectedIdentityPayload
        {
            ExternalId =
                user.ObjectGuid.Trim(),

            DisplayName =
                GetDisplayName(user),

            UserName =
                GetUserName(user),

            Email =
                NormalizeOptionalText(
                    user.Email),

            Source =
                "ActiveDirectory",

            AccountType =
                isServiceAccount
                    ? "ServiceAccount"
                    : "User",

            IsEnabled =
                isEnabled,

            // Le privilège dépendra des groupes
            // et appartenances collectés ensuite.
            IsPrivileged =
                false,

            IsServiceAccount =
                isServiceAccount,

            IsLocked =
                isLocked,

            LastSignInAt =
                user.LastLogonAt,

            Description =
                NormalizeOptionalText(
                    user.Description),

            Owner =
                null
        };
    }

    private static bool IsServiceAccount(
        ActiveDirectoryUser user)
    {
        var samAccountName =
            user.SamAccountName.Trim();

        if (samAccountName.StartsWith(
            "svc.",
            StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var description =
            NormalizeOptionalText(
                user.Description);

        if (description is not null &&
            description.Contains(
                "service account",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static string GetDisplayName(
        ActiveDirectoryUser user)
    {
        if (!string.IsNullOrWhiteSpace(
            user.DisplayName))
        {
            return user.DisplayName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(
            user.UserPrincipalName))
        {
            return user.UserPrincipalName.Trim();
        }

        return user.SamAccountName.Trim();
    }

    private static string GetUserName(
        ActiveDirectoryUser user)
    {
        return string.IsNullOrWhiteSpace(
            user.UserPrincipalName)
            ? user.SamAccountName.Trim()
            : user.UserPrincipalName.Trim();
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}