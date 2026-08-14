namespace IdentityAudit.ActiveDirectoryCollector.Models;

public sealed class ActiveDirectoryUser
{
    public string ObjectGuid { get; init; } = string.Empty;

    public string DistinguishedName { get; init; } = string.Empty;

    public string SamAccountName { get; init; } = string.Empty;

    public string? UserPrincipalName { get; init; }

    public string? DisplayName { get; init; }

    public string? Email { get; init; }

    public int UserAccountControl { get; init; }

    public int? ComputedUserAccountControl { get; init; }

    public DateTimeOffset? LastLogonAt { get; init; }

    public string? Description { get; init; }

    public IReadOnlyCollection<string> ServicePrincipalNames
    {
        get;
        init;
    } = Array.Empty<string>();
}