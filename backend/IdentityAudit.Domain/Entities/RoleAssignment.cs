namespace IdentityAudit.Domain.Entities;

public sealed class RoleAssignment
{
    public Guid Id { get; set; }

    public Guid IdentityId { get; set; }

    public DirectoryIdentity Identity { get; set; }
        = null!;

    public Guid DirectoryRoleId { get; set; }

    public DirectoryRole DirectoryRole { get; set; }
        = null!;

    public DateTimeOffset? AssignedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public bool IsPermanent { get; set; }

    public DateTimeOffset CollectedAt { get; set; }
}