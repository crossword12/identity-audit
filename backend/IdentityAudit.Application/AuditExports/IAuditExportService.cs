namespace IdentityAudit.Application.AuditExports;

public interface IAuditExportService
{
    Task<AuditCsvExportResult> ExportCsvAsync(
        Guid auditId,
        CancellationToken cancellationToken = default);
}