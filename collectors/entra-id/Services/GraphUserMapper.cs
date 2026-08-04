using IdentityAudit.EntraCollector.Models;

namespace IdentityAudit.EntraCollector.Services;

public sealed class GraphUserMapper
{
    public IReadOnlyCollection<CollectedIdentityPayload> Map(
        IEnumerable<GraphUser> graphUsers)
    {
        ArgumentNullException.ThrowIfNull(graphUsers);

        return graphUsers
            .Select(MapUser)
            .ToList();
    }

    private static CollectedIdentityPayload MapUser(
        GraphUser graphUser)
    {
        var isGuest = graphUser.UserType.Equals(
            "Guest",
            StringComparison.OrdinalIgnoreCase);

        var isServiceAccount =
            graphUser.EmployeeType?.Equals(
                "ServiceAccount",
                StringComparison.OrdinalIgnoreCase)
            == true;

        var accountType = GetAccountType(
            isGuest,
            isServiceAccount);

        return new CollectedIdentityPayload
        {
            ExternalId = graphUser.Id.Trim(),

            DisplayName = NormalizeRequiredText(
                graphUser.DisplayName,
                graphUser.UserPrincipalName),

            UserName = graphUser.UserPrincipalName.Trim(),

            Email = NormalizeOptionalText(graphUser.Mail),

            Source = "EntraId",

            AccountType = accountType,

            IsEnabled = graphUser.AccountEnabled,

            IsPrivileged = false,

            IsServiceAccount = isServiceAccount,

            IsLocked = null,

            LastSignInAt = graphUser
                .SignInActivity?
                .LastSuccessfulSignInDateTime,

            Description = null,

            Owner = null
        };
    }

    private static string GetAccountType(
        bool isGuest,
        bool isServiceAccount)
    {
        if (isServiceAccount)
        {
            return "ServiceAccount";
        }

        if (isGuest)
        {
            return "Guest";
        }

        return "User";
    }

    private static string NormalizeRequiredText(
        string? value,
        string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback.Trim()
            : value.Trim();
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}