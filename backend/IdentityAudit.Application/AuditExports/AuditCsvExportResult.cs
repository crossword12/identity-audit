namespace IdentityAudit.Application.AuditExports;

public sealed record AuditCsvExportResult(
    bool Succeeded,
    byte[]? Content,
    string? FileName,
    string? ErrorMessage)
{
    public static AuditCsvExportResult Success(
        byte[] content,
        string fileName)
    {
        return new AuditCsvExportResult(
            true,
            content,
            fileName,
            null);
    }

    public static AuditCsvExportResult Failure(
        string errorMessage)
    {
        return new AuditCsvExportResult(
            false,
            null,
            null,
            errorMessage);
    }
}