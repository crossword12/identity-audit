namespace IdentityAudit.Application.Audits;

public interface IAuditService
{
    Task<IReadOnlyList<AuditDto>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<AuditDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<CreateAuditResult> CreateAsync(
        CreateAuditRequest request,
        CancellationToken cancellationToken);
}