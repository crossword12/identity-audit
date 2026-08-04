using IdentityAudit.EntraCollector.Models;

namespace IdentityAudit.EntraCollector.Services;

public sealed class GraphRoleDefinitionMapper
{
    private static readonly HashSet<string>
        PrivilegedRoleExternalIds =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "dddddddd-dddd-dddd-dddd-dddddddddddd",
                "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"
            };

    public IReadOnlyCollection<CollectedRolePayload> Map(
        IEnumerable<GraphRoleDefinition> graphRoles)
    {
        ArgumentNullException.ThrowIfNull(graphRoles);

        return graphRoles
            .Where(role => role.IsEnabled)
            .Where(role =>
                !string.IsNullOrWhiteSpace(role.Id))
            .Select(MapRole)
            .ToList();
    }

    private static CollectedRolePayload MapRole(
        GraphRoleDefinition graphRole)
    {
        var externalId =
            graphRole.Id.Trim();

        return new CollectedRolePayload
        {
            ExternalId = externalId,

            Name = string.IsNullOrWhiteSpace(
                graphRole.DisplayName)
                ? externalId
                : graphRole.DisplayName.Trim(),

            Description =
                string.IsNullOrWhiteSpace(
                    graphRole.Description)
                    ? null
                    : graphRole.Description.Trim(),

            Source = "EntraId",

            IsPrivileged =
                PrivilegedRoleExternalIds.Contains(
                    externalId)
        };
    }
}