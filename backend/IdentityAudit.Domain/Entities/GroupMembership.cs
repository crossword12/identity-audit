namespace IdentityAudit.Domain.Entities;

public sealed class GroupMembership
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid IdentityId { get; set; }

    public DirectoryIdentity Identity { get; set; } = null!;

    public Guid GroupId { get; set; }

    public DirectoryGroup Group { get; set; } = null!;

    public string MembershipType { get; set; } = "Direct";

    public DateTimeOffset CollectedAt { get; set; }
        = DateTimeOffset.UtcNow;
}