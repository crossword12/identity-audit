namespace IdentityAudit.EntraCollector.Models;

public sealed class GraphGroup
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public IReadOnlyCollection<string> GroupTypes { get; init; }
        = Array.Empty<string>();

    public bool MailEnabled { get; init; }

    public bool SecurityEnabled { get; init; }

    public bool IsAssignableToRole { get; init; }
}