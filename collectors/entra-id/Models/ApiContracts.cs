namespace IdentityAudit.EntraCollector.Models;

public sealed class CreateAuditPayload
{
    public Guid TargetId { get; init; }
}

public sealed class AuditApiResponse
{
    public Guid Id { get; init; }

    public Guid TargetId { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }
}

public sealed class ImportIdentitiesPayload
{
    public IReadOnlyCollection<CollectedIdentityPayload> Identities
    {
        get;
        init;
    } = Array.Empty<CollectedIdentityPayload>();
}

public sealed class ImportIdentitiesApiResponse
{
    public int ImportedCount { get; init; }
}

public sealed class ImportGroupsPayload
{
    public IReadOnlyCollection<CollectedGroupPayload> Groups
    {
        get;
        init;
    } = Array.Empty<CollectedGroupPayload>();
}

public sealed class ImportGroupsApiResponse
{
    public int ImportedCount { get; init; }
}

public sealed class ImportGroupMembershipsPayload
{
    public IReadOnlyCollection<CollectedGroupMembershipPayload>
        Memberships
    {
        get;
        init;
    } = Array.Empty<CollectedGroupMembershipPayload>();
}

public sealed class ImportGroupMembershipsApiResponse
{
    public int ImportedCount { get; init; }
}

public sealed class ImportRolesPayload
{
    public IReadOnlyCollection<CollectedRolePayload> Roles
    {
        get;
        init;
    } = Array.Empty<CollectedRolePayload>();
}

public sealed class ImportRolesApiResponse
{
    public int ImportedCount { get; init; }
}

public sealed class ImportRoleAssignmentsPayload
{
    public IReadOnlyCollection<
        CollectedRoleAssignmentPayload> Assignments
    {
        get;
        init;
    } = Array.Empty<
        CollectedRoleAssignmentPayload>();
}

public sealed class ImportRoleAssignmentsApiResponse
{
    public int ImportedCount { get; init; }
}