using System.Globalization;
using System.Text;
using IdentityAudit.Application.AuditExports;
using IdentityAudit.Application.Audits;
using IdentityAudit.Application.RuleEvaluations;
using IdentityAudit.Infrastructure.Exports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace IdentityAudit.Infrastructure.Services;

public sealed class AuditExportService : IAuditExportService
{
    private readonly IAuditService _auditService;
    private readonly IAuditRuleEvaluationService
        _ruleEvaluationService;

    public AuditExportService(
        IAuditService auditService,
        IAuditRuleEvaluationService ruleEvaluationService)
    {
        _auditService = auditService;
        _ruleEvaluationService = ruleEvaluationService;
    }

    public async Task<AuditCsvExportResult> ExportCsvAsync(
        Guid auditId,
        CancellationToken cancellationToken = default)
    {
        var audit = await _auditService.GetByIdAsync(
            auditId,
            cancellationToken);

        if (audit is null)
        {
            return AuditCsvExportResult.Failure(
                "L'audit demandé est introuvable.");
        }

        var evaluations =
            await _ruleEvaluationService.GetByAuditAsync(
                auditId,
                cancellationToken);

        var csv = new StringBuilder();

        AppendRow(
            csv,
            "IdentifiantAudit",
            "Cible",
            "StatutAudit",
            "ScoreConformite",
            "CodeRegle",
            "NomRegle",
            "ControleCIS",
            "TypeCible",
            "Severite",
            "StatutEvaluation",
            "NombreConstats",
            "Recommandation",
            "PreuvesJson",
            "Erreur",
            "EvalueLe");

        foreach (var evaluation in evaluations
                     .OrderBy(item => item.RuleCode))
        {
            AppendRow(
                csv,
                audit.Id.ToString(),
                audit.TargetName,
                audit.Status.ToString(),
                FormatScore(audit.ComplianceScore),
                evaluation.RuleCode,
                evaluation.RuleName,
                evaluation.CisControl,
                evaluation.TargetType.ToString(),
                evaluation.Severity.ToString(),
                evaluation.Status.ToString(),
                evaluation.FindingCount.ToString(
                    CultureInfo.InvariantCulture),
                evaluation.Recommendation,
                evaluation.EvidenceJson,
                evaluation.ErrorMessage,
                evaluation.EvaluatedAt
                    .ToUniversalTime()
                    .ToString(
                        "O",
                        CultureInfo.InvariantCulture));
        }

        var fileName =
            $"audit-{audit.Id:N}-resultats-cis.csv";

        return AuditCsvExportResult.Success(
            EncodeWithUtf8Bom(csv.ToString()),
            fileName);
    }

    public async Task<AuditPdfExportResult> ExportPdfAsync(
    Guid auditId,
    CancellationToken cancellationToken = default)
    {
        var audit = await _auditService.GetByIdAsync(
            auditId,
            cancellationToken);

        if (audit is null)
        {
            return AuditPdfExportResult.Failure(
                "L'audit demandé est introuvable.");
        }

        var evaluations =
            await _ruleEvaluationService.GetByAuditAsync(
                auditId,
                cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        var orderedEvaluations =
            evaluations
                .OrderBy(evaluation => evaluation.RuleCode)
                .ToArray();

        QuestPDF.Settings.License =
            LicenseType.Community;

        var document = new AuditPdfDocument(
            audit,
            orderedEvaluations,
            DateTimeOffset.UtcNow);

        var content =
            document.GeneratePdf();

        var fileName =
            $"audit-{audit.Id:N}-rapport.pdf";

        return AuditPdfExportResult.Success(
            content,
            fileName);
    }

    private static string FormatScore(
        decimal? score)
    {
        return score?.ToString(
                   "0.00",
                   CultureInfo.InvariantCulture)
               ?? string.Empty;
    }

    private static void AppendRow(
        StringBuilder csv,
        params string?[] values)
    {
        csv.AppendLine(
            string.Join(
                ';',
                values.Select(EscapeCell)));
    }

    private static string EscapeCell(
        string? value)
    {
        var safeValue =
            value ?? string.Empty;

        var candidate =
            safeValue.TrimStart();

        if (candidate.Length > 0 &&
            candidate[0] is '=' or '+' or '-' or '@')
        {
            safeValue =
                $"'{safeValue}";
        }

        return
            $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }

    private static byte[] EncodeWithUtf8Bom(
        string content)
    {
        var encoding =
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: true);

        var preamble =
            encoding.GetPreamble();

        var body =
            encoding.GetBytes(content);

        var result =
            new byte[preamble.Length + body.Length];

        Array.Copy(
            preamble,
            0,
            result,
            0,
            preamble.Length);

        Array.Copy(
            body,
            0,
            result,
            preamble.Length,
            body.Length);

        return result;
    }
}
