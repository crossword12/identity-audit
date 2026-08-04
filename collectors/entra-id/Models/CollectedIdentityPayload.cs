namespace IdentityAudit.EntraCollector.Models;

public sealed class CollectedIdentityPayload
{
    public string ExternalId { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string? Email { get; init; }

    public string Source { get; init; } = "EntraId";

    public string AccountType { get; init; } = "User";

    public bool IsEnabled { get; init; }

    public bool IsPrivileged { get; init; }

    public bool IsServiceAccount { get; init; }

    public bool? IsLocked { get; init; }

    public DateTimeOffset? LastSignInAt { get; init; }

    public string? Description { get; init; }

    public string? Owner { get; init; }
}