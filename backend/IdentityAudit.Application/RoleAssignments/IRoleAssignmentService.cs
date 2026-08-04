namespace IdentityAudit.Application.RoleAssignments;

public interface IRoleAssignmentService
{
    Task<IReadOnlyList<RoleAssignmentDto>>
        GetByAuditIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default);

    Task<ImportRoleAssignmentsResult> ImportAsync(
        Guid auditId,
        ImportRoleAssignmentsRequest request,
        CancellationToken cancellationToken = default);
}