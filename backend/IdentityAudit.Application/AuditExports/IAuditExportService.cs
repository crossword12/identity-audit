namespace IdentityAudit.Application.AuditExports;

public interface IAuditExportService
{
    Task<AuditCsvExportResult> ExportCsvAsync(
        Guid auditId,
        CancellationToken cancellationToken = default);

    Task<AuditPdfExportResult> ExportPdfAsync(
        Guid auditId,
        CancellationToken cancellationToken = default);
}