using IdentityAudit.EntraCollector.Models;

namespace IdentityAudit.EntraCollector.Services;

public sealed class GraphRoleAssignmentMapper
{
    public IReadOnlyCollection<
        CollectedRoleAssignmentPayload> Map(
            IEnumerable<GraphRoleAssignment>
                graphAssignments,
            IReadOnlySet<string>
                knownIdentityExternalIds,
            IReadOnlySet<string>
                knownRoleExternalIds)
    {
        ArgumentNullException.ThrowIfNull(
            graphAssignments);

        ArgumentNullException.ThrowIfNull(
            knownIdentityExternalIds);

        ArgumentNullException.ThrowIfNull(
            knownRoleExternalIds);

        return graphAssignments
            .Where(assignment =>
                !string.IsNullOrWhiteSpace(
                    assignment.PrincipalId))
            .Where(assignment =>
                !string.IsNullOrWhiteSpace(
                    assignment.RoleDefinitionId))
            .Where(assignment =>
                knownIdentityExternalIds.Contains(
                    assignment.PrincipalId.Trim()))
            .Where(assignment =>
                knownRoleExternalIds.Contains(
                    assignment.RoleDefinitionId.Trim()))
            .GroupBy(
                assignment =>
                    $"{assignment.PrincipalId.Trim()}" +
                    "\u001F" +
                    $"{assignment.RoleDefinitionId.Trim()}",
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Select(assignment =>
                new CollectedRoleAssignmentPayload
                {
                    IdentityExternalId =
                        assignment.PrincipalId.Trim(),

                    RoleExternalId =
                        assignment.RoleDefinitionId.Trim(),

                    AssignedAt =
                        assignment.StartDateTime,

                    ExpiresAt =
                        assignment.EndDateTime,

                    IsPermanent =
                        !assignment.EndDateTime.HasValue
                })
            .ToList();
    }
}