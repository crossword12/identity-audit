namespace IdentityAudit.EntraCollector.Models;

public sealed class GraphRoleAssignment
{
    public string Id { get; init; } = string.Empty;

    public string PrincipalId { get; init; } = string.Empty;

    public string RoleDefinitionId { get; init; }
        = string.Empty;

    public string DirectoryScopeId { get; init; }
        = string.Empty;

    public DateTimeOffset? StartDateTime { get; init; }

    public DateTimeOffset? EndDateTime { get; init; }
}