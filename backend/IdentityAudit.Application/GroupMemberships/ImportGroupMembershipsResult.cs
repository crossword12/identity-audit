namespace IdentityAudit.Application.GroupMemberships;

public sealed class ImportGroupMembershipsResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyList<GroupMembershipDto> Memberships
    {
        get;
        init;
    } = Array.Empty<GroupMembershipDto>();

    public int ImportedCount => Memberships.Count;

    public static ImportGroupMembershipsResult Success(
        IReadOnlyList<GroupMembershipDto> memberships)
    {
        return new ImportGroupMembershipsResult
        {
            Succeeded = true,
            Memberships = memberships
        };
    }

    public static ImportGroupMembershipsResult Failure(
        string errorMessage)
    {
        return new ImportGroupMembershipsResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}