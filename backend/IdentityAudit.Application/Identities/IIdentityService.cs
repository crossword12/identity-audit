namespace IdentityAudit.Application.Identities;

public interface IIdentityService
{
    Task<IReadOnlyList<DirectoryIdentityDto>> GetByAuditIdAsync(
        Guid auditId,
        CancellationToken cancellationToken = default);

    Task<ImportIdentitiesResult> ImportAsync(
        Guid auditId,
        ImportIdentitiesRequest request,
        CancellationToken cancellationToken = default);
}