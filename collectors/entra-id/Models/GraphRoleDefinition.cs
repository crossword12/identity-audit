namespace IdentityAudit.EntraCollector.Models;

public sealed class GraphRoleDefinition
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsBuiltIn { get; init; }

    public bool IsEnabled { get; init; }
}