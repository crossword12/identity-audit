namespace IdentityAudit.Application.AuditExports;

public sealed record AuditPdfExportResult(
    bool Succeeded,
    byte[]? Content,
    string? FileName,
    string? ErrorMessage)
{
    public static AuditPdfExportResult Success(
        byte[] content,
        string fileName)
    {
        return new AuditPdfExportResult(
            true,
            content,
            fileName,
            null);
    }

    public static AuditPdfExportResult Failure(
        string errorMessage)
    {
        return new AuditPdfExportResult(
            false,
            null,
            null,
            errorMessage);
    }
}