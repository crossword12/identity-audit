namespace IdentityAudit.EntraCollector.Models;

public sealed class GraphUser
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string UserPrincipalName { get; init; } = string.Empty;

    public string? Mail { get; init; }

    public string UserType { get; init; } = string.Empty;

    public bool AccountEnabled { get; init; }

    public string? EmployeeType { get; init; }

    public GraphSignInActivity? SignInActivity { get; init; }
}

public sealed class GraphSignInActivity
{
    public DateTimeOffset? LastSuccessfulSignInDateTime
    {
        get;
        init;
    }
}