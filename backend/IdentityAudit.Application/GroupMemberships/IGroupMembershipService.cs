namespace IdentityAudit.Application.GroupMemberships;

public interface IGroupMembershipService
{
    Task<IReadOnlyList<GroupMembershipDto>>
        GetByAuditIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default);

    Task<ImportGroupMembershipsResult> ImportAsync(
        Guid auditId,
        ImportGroupMembershipsRequest request,
        CancellationToken cancellationToken = default);
}