namespace IdentityAudit.Application.GroupMemberships;

public sealed class GroupMembershipDto
{
    public Guid Id { get; init; }

    public Guid AuditId { get; init; }

    public Guid IdentityId { get; init; }

    public string IdentityExternalId { get; init; }
        = string.Empty;

    public string IdentityDisplayName { get; init; }
        = string.Empty;

    public Guid GroupId { get; init; }

    public string GroupExternalId { get; init; }
        = string.Empty;

    public string GroupName { get; init; }
        = string.Empty;

    public string MembershipType { get; init; }
        = string.Empty;

    public DateTimeOffset CollectedAt { get; init; }
}