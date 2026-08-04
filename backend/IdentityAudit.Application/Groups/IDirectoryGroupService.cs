namespace IdentityAudit.Application.Groups;

public interface IDirectoryGroupService
{
    Task<IReadOnlyList<DirectoryGroupDto>> GetByAuditIdAsync(
        Guid auditId,
        CancellationToken cancellationToken = default);

    Task<ImportGroupsResult> ImportAsync(
        Guid auditId,
        ImportGroupsRequest request,
        CancellationToken cancellationToken = default);
}