using IdentityAudit.EntraCollector.Models;

namespace IdentityAudit.EntraCollector.Services;

public sealed class GraphGroupMapper
{
    public IReadOnlyCollection<CollectedGroupPayload> Map(
        IEnumerable<GraphGroup> graphGroups)
    {
        ArgumentNullException.ThrowIfNull(graphGroups);

        return graphGroups
            .Select(MapGroup)
            .ToList();
    }

    private static CollectedGroupPayload MapGroup(
        GraphGroup graphGroup)
    {
        var groupType = ResolveGroupType(graphGroup);

        return new CollectedGroupPayload
        {
            ExternalId = graphGroup.Id.Trim(),

            Name = string.IsNullOrWhiteSpace(
                graphGroup.DisplayName)
                ? graphGroup.Id.Trim()
                : graphGroup.DisplayName.Trim(),

            Description = NormalizeOptionalText(
                graphGroup.Description),

            Source = "EntraId",

            GroupType = groupType,

            IsPrivileged =
                graphGroup.IsAssignableToRole
        };
    }

    private static string ResolveGroupType(
        GraphGroup graphGroup)
    {
        var isMicrosoft365 =
            graphGroup.GroupTypes.Any(groupType =>
                groupType.Equals(
                    "Unified",
                    StringComparison.OrdinalIgnoreCase));

        if (isMicrosoft365)
        {
            return "Microsoft365";
        }

        if (graphGroup.SecurityEnabled)
        {
            return "Security";
        }

        if (graphGroup.MailEnabled)
        {
            return "Distribution";
        }

        return "Other";
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}