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

    Task<StartAuditResult> StartAsync(
    Guid auditId,
    CancellationToken cancellationToken = default);

    Task<CompleteAuditResult> CompleteAsync(
    Guid auditId,
    CancellationToken cancellationToken = default);
}