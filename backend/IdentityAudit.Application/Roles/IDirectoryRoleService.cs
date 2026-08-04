namespace IdentityAudit.Application.Roles;

public interface IDirectoryRoleService
{
    Task<IReadOnlyList<DirectoryRoleDto>>
        GetByAuditIdAsync(
            Guid auditId,
            CancellationToken cancellationToken = default);

    Task<ImportRolesResult> ImportAsync(
        Guid auditId,
        ImportRolesRequest request,
        CancellationToken cancellationToken = default);
}