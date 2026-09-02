using System.Globalization;
using IdentityAudit.Application.Audits;
using IdentityAudit.Application.RuleEvaluations;
using IdentityAudit.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using System.Text;

namespace IdentityAudit.Infrastructure.Exports;

internal sealed class AuditPdfDocument : IDocument
{
    private const string PrimaryColor = "#1F4E78";
    private const string SecondaryColor = "#2F75B5";
    private const string BackgroundColor = "#F5F7FA";
    private const string TextColor = "#1F2937";
    private const string MutedColor = "#6B7280";
    private const string SuccessColor = "#15803D";
    private const string DangerColor = "#DC2626";
    private const string WarningColor = "#D97706";

    private static readonly CultureInfo FrenchCulture =
        CultureInfo.GetCultureInfo("fr-FR");

    private readonly AuditDto _audit;
    private readonly IReadOnlyList<RuleEvaluationDto>
        _evaluations;
    private readonly DateTimeOffset _generatedAt;

    public AuditPdfDocument(
        AuditDto audit,
        IReadOnlyList<RuleEvaluationDto> evaluations,
        DateTimeOffset generatedAt)
    {
        _audit = audit;
        _evaluations = evaluations;
        _generatedAt = generatedAt;
    }

    public DocumentMetadata GetMetadata()
    {
        return new DocumentMetadata
        {
            Title =
                $"Rapport d'audit {_audit.Id}",
            Author = "Identity Audit",
            Subject =
                "Rapport de conformité CIS Controls 5 et 6",
            Keywords =
                "Identity Audit, CIS Controls, identités, privilèges"
        };
    }

    public void Compose(
        IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(
                1.6f,
                Unit.Centimetre);

            page.PageColor(Colors.White);

            page.DefaultTextStyle(
                style => style
                    .FontSize(9)
                    .FontColor(TextColor));

            page.Header()
                .Element(ComposeHeader);

            page.Content()
                .PaddingVertical(12)
                .Element(ComposeContent);

            page.Footer()
                .Element(ComposeFooter);
        });
    }

    private void ComposeHeader(
    IContainer container)
    {
        container
            .BorderBottom(1)
            .BorderColor("#DCE3EA")
            .PaddingBottom(10)
            .AlignCenter()
            .Text("Rapport d’audit de conformité")
            .Bold()
            .FontSize(16)
            .FontColor(PrimaryColor);
    }

    private void ComposeContent(
        IContainer container)
    {
        var compliantCount =
            _evaluations.Count(
                evaluation =>
                    evaluation.Status ==
                    RuleEvaluationStatus.Compliant);

        var nonCompliantCount =
            _evaluations.Count(
                evaluation =>
                    evaluation.Status ==
                    RuleEvaluationStatus.NonCompliant);

        var notApplicableCount =
            _evaluations.Count(
                evaluation =>
                    evaluation.Status ==
                    RuleEvaluationStatus.NotApplicable);

        var notVerifiableCount =
            _evaluations.Count(
                evaluation =>
                    evaluation.Status is
                        RuleEvaluationStatus.NotVerifiable or
                        RuleEvaluationStatus.Error);

        container.Column(column =>
        {
            column.Spacing(16);

            // column.Item()
            //     .Text("Rapport d’audit de conformité")
            //     .Bold()
            //     .FontSize(20)
            //     .FontColor(PrimaryColor);

            column.Item()
                .Text(
                    $"Référence : {_audit.Id}")
                .FontSize(9)
                .FontColor(MutedColor);

            column.Item()
                .Text("Informations générales")
                .Bold()
                .FontSize(13)
                .FontColor(PrimaryColor);

            column.Item()
                .Background(BackgroundColor)
                .Border(1)
                .BorderColor("#DCE3EA")
                .Padding(12)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(125);
                        columns.RelativeColumn();
                    });

                    AddInformationRow(
                        table,
                        "Cible",
                        _audit.TargetName);

                    AddInformationRow(
                        table,
                        "Statut",
                        FormatAuditStatus(_audit.Status));

                    AddInformationRow(
                        table,
                        "Créé le",
                        FormatDate(_audit.CreatedAt));

                    AddInformationRow(
                        table,
                        "Démarré le",
                        FormatDate(_audit.StartedAt));

                    AddInformationRow(
                        table,
                        "Terminé le",
                        FormatDate(_audit.CompletedAt));
                });

            column.Item()
                .Text("Synthèse de conformité")
                .Bold()
                .FontSize(13)
                .FontColor(PrimaryColor);

            column.Item()
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    AddMetric(
                        table,
                        FormatScore(_audit.ComplianceScore),
                        "Score",
                        SecondaryColor);

                    AddMetric(
                        table,
                        _evaluations.Count.ToString(
                            FrenchCulture),
                        "Règles évaluées",
                        PrimaryColor);

                    AddMetric(
                        table,
                        compliantCount.ToString(
                            FrenchCulture),
                        "Conformes",
                        SuccessColor);

                    AddMetric(
                        table,
                        nonCompliantCount.ToString(
                            FrenchCulture),
                        "Non conformes",
                        DangerColor);

                    AddMetric(
                        table,
                        notApplicableCount.ToString(
                            FrenchCulture),
                        "Non applicables",
                        MutedColor);

                    AddMetric(
                        table,
                        notVerifiableCount.ToString(
                            FrenchCulture),
                        "Non vérifiables / erreurs",
                        WarningColor);
                });
            column.Item()
                .PaddingTop(4)
                .Text("Résultats des règles CIS")
                .Bold()
                .FontSize(13)
                .FontColor(PrimaryColor);

            column.Item()
                .Element(ComposeEvaluationsTable);

            column.Item()
                .PageBreak();

            column.Item()
                .Text("Détail des non-conformités")
                .Bold()
                .FontSize(15)
                .FontColor(PrimaryColor);

            column.Item()
                .Text(
                    "Cette section présente les constats, " +
                    "les recommandations et les preuves collectées.")
                .FontSize(9)
                .FontColor(MutedColor);

            column.Item()
                .Element(ComposeFindingsSection);
        });
    }

    private void ComposeEvaluationsTable(
    IContainer container)
    {
        if (_evaluations.Count == 0)
        {
            container
                .Background(BackgroundColor)
                .Border(1)
                .BorderColor("#DCE3EA")
                .Padding(12)
                .Text(
                    "Aucun résultat de règle n'est disponible.")
                .FontColor(MutedColor);

            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(52);
                columns.RelativeColumn(2.4f);
                columns.ConstantColumn(62);
                columns.ConstantColumn(58);
                columns.ConstantColumn(72);
                columns.ConstantColumn(46);
            });

            table.Header(header =>
            {
                header.Cell()
                    .Element(StyleHeaderCell)
                    .Text("Code")
                    .SemiBold()
                    .FontColor(Colors.White);

                header.Cell()
                    .Element(StyleHeaderCell)
                    .Text("Règle")
                    .SemiBold()
                    .FontColor(Colors.White);

                header.Cell()
                    .Element(StyleHeaderCell)
                    .Text("Contrôle")
                    .SemiBold()
                    .FontColor(Colors.White);

                header.Cell()
                    .Element(StyleHeaderCell)
                    .Text("Sévérité")
                    .SemiBold()
                    .FontColor(Colors.White);

                header.Cell()
                    .Element(StyleHeaderCell)
                    .Text("Statut")
                    .SemiBold()
                    .FontColor(Colors.White);

                header.Cell()
                    .Element(StyleHeaderCell)
                    .AlignCenter()
                    .Text("Constats")
                    .SemiBold()
                    .FontColor(Colors.White);
            });

            for (var index = 0;
                 index < _evaluations.Count;
                 index++)
            {
                var evaluation =
                    _evaluations[index];

                var backgroundColor =
                    index % 2 == 0
                        ? "#FFFFFF"
                        : BackgroundColor;

                table.Cell()
                    .Element(
                        cell => StyleBodyCell(
                            cell,
                            backgroundColor))
                    .Text(evaluation.RuleCode)
                    .SemiBold();

                table.Cell()
                    .Element(
                        cell => StyleBodyCell(
                            cell,
                            backgroundColor))
                    .Text(evaluation.RuleName);

                table.Cell()
                    .Element(
                        cell => StyleBodyCell(
                            cell,
                            backgroundColor))
                    .Text(evaluation.CisControl);

                table.Cell()
                    .Element(
                        cell => StyleBodyCell(
                            cell,
                            backgroundColor))
                    .Text(
                        FormatSeverity(
                            evaluation.Severity));

                table.Cell()
                    .Element(
                        cell => StyleBodyCell(
                            cell,
                            backgroundColor))
                    .Text(
                        FormatEvaluationStatus(
                            evaluation.Status))
                    .SemiBold()
                    .FontColor(
                        GetEvaluationStatusColor(
                            evaluation.Status));

                table.Cell()
                    .Element(
                        cell => StyleBodyCell(
                            cell,
                            backgroundColor))
                    .AlignCenter()
                    .Text(
                        evaluation.FindingCount.ToString(
                            FrenchCulture))
                    .SemiBold();
            }
        });
    }

    private void ComposeFindingsSection(
    IContainer container)
    {
        var findings =
            _evaluations
                .Where(evaluation =>
                    evaluation.Status is
                        RuleEvaluationStatus.NonCompliant or
                        RuleEvaluationStatus.NotVerifiable or
                        RuleEvaluationStatus.Error)
                .OrderBy(evaluation => evaluation.RuleCode)
                .ToArray();

        if (findings.Length == 0)
        {
            container
                .Background("#F0FDF4")
                .Border(1)
                .BorderColor("#BBF7D0")
                .Padding(12)
                .Text(
                    "Aucune non-conformité ou erreur " +
                    "n'a été détectée.")
                .FontColor(SuccessColor);

            return;
        }

        container.Column(column =>
        {
            column.Spacing(12);

            foreach (var evaluation in findings)
            {
                column.Item()
                    .Element(
                        item => ComposeFindingCard(
                            item,
                            evaluation));
            }
        });
    }

    private static void ComposeFindingCard(
        IContainer container,
        RuleEvaluationDto evaluation)
    {
        var accentColor =
            GetEvaluationStatusColor(
                evaluation.Status);

        var backgroundColor =
            evaluation.Status switch
            {
                RuleEvaluationStatus.NonCompliant =>
                    "#FEF2F2",

                RuleEvaluationStatus.NotVerifiable =>
                    "#FFFBEB",

                RuleEvaluationStatus.Error =>
                    "#FEF2F2",

                _ => BackgroundColor
            };

        container
            .BorderLeft(4)
            .BorderColor(accentColor)
            .Background(backgroundColor)
            .Padding(10)
            .Column(column =>
            {
                column.Spacing(6);

                column.Item()
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text(
                                $"{evaluation.RuleCode} — " +
                                $"{evaluation.RuleName}")
                            .Bold()
                            .FontSize(11)
                            .FontColor(PrimaryColor);

                        row.ConstantItem(90)
                            .AlignRight()
                            .Text(
                                FormatEvaluationStatus(
                                    evaluation.Status))
                            .SemiBold()
                            .FontSize(8)
                            .FontColor(accentColor);
                    });

                column.Item()
                    .DefaultTextStyle(
                        style => style.FontSize(8))
                    .Text(text =>
                    {
                        text.Span("Sévérité : ")
                            .SemiBold();

                        text.Span(
                            FormatSeverity(
                                evaluation.Severity));

                        text.Span("    Constats : ")
                            .SemiBold();

                        text.Span(
                            evaluation.FindingCount.ToString(
                                FrenchCulture));
                    });

                column.Item()
                    .Text("Recommandation")
                    .SemiBold()
                    .FontSize(9)
                    .FontColor(PrimaryColor);

                column.Item()
                    .Text(
                        string.IsNullOrWhiteSpace(
                            evaluation.Recommendation)
                            ? "Aucune recommandation disponible."
                            : evaluation.Recommendation)
                    .FontSize(8);

                if (!string.IsNullOrWhiteSpace(
                        evaluation.ErrorMessage))
                {
                    column.Item()
                        .Text("Erreur d’évaluation")
                        .SemiBold()
                        .FontSize(9)
                        .FontColor(DangerColor);

                    column.Item()
                        .Text(evaluation.ErrorMessage)
                        .FontSize(8)
                        .FontColor(DangerColor);
                }

                column.Item()
                    .Text("Preuves collectées")
                    .SemiBold()
                    .FontSize(9)
                    .FontColor(PrimaryColor);

                column.Item()
                    .Background(Colors.White)
                    .Border(1)
                    .BorderColor("#DCE3EA")
                    .Padding(6)
                    .Text(
                        FormatEvidenceJson(
                            evaluation.EvidenceJson))
                    .FontSize(7)
                    .FontColor(MutedColor);
            });
    }

    private static string FormatEvidenceJson(
    string? evidenceJson)
    {
        if (string.IsNullOrWhiteSpace(evidenceJson))
        {
            return "Aucune preuve détaillée disponible.";
        }

        try
        {
            using var document =
                JsonDocument.Parse(evidenceJson);

            var builder =
                new StringBuilder();

            if (document.RootElement.ValueKind ==
                JsonValueKind.Array)
            {
                var index = 1;

                foreach (var item in
                         document.RootElement.EnumerateArray())
                {
                    AppendEvidenceItem(
                        builder,
                        item,
                        index);

                    index++;
                }
            }
            else
            {
                AppendEvidenceItem(
                    builder,
                    document.RootElement,
                    1);
            }

            return builder.Length > 0
                ? builder.ToString().Trim()
                : "Aucune preuve détaillée disponible.";
        }
        catch (JsonException)
        {
            return evidenceJson;
        }
    }

    private static void AppendEvidenceItem(
    StringBuilder builder,
    JsonElement item,
    int index)
    {
        if (index > 1)
        {
            builder.AppendLine();
        }

        builder.AppendLine(
            $"Constat {index}");

        var hasDetails = false;

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "displayName",
            "Identité");

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "userName",
            "Compte");

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "groupName",
            "Groupe");

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "roleName",
            "Rôle");

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "reason",
            "Motif");

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "membershipType",
            "Type d’appartenance");

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "owner",
            "Propriétaire",
            includeNull: true);

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "description",
            "Description");

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "externalId",
            "Identifiant externe");

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "groupExternalId",
            "Identifiant du groupe");

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "assignedAt",
            "Date d’attribution");

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "expiresAt",
            "Date d’expiration",
            includeNull: true);

        hasDetails |= AppendEvidenceValue(
            builder,
            item,
            "isPermanent",
            "Attribution permanente");

        hasDetails |= AppendEvidenceCollection(
            builder,
            item,
            "privilegedGroups",
            "Groupes privilégiés");

        hasDetails |= AppendEvidenceCollection(
            builder,
            item,
            "privilegedRoles",
            "Rôles privilégiés");

        if (!hasDetails)
        {
            builder.AppendLine(
                item.GetRawText());
        }
    }

    private static bool AppendEvidenceValue(
    StringBuilder builder,
    JsonElement item,
    string propertyName,
    string label,
    bool includeNull = false)
    {
        if (item.ValueKind != JsonValueKind.Object ||
            !item.TryGetProperty(
                propertyName,
                out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Null)
        {
            if (!includeNull)
            {
                return false;
            }

            builder.AppendLine(
                $"• {label} : non renseigné");

            return true;
        }

        var value =
            property.ValueKind switch
            {
                JsonValueKind.String =>
                    property.GetString(),

                JsonValueKind.Number =>
                    property.GetRawText(),

                JsonValueKind.True =>
                    "Oui",

                JsonValueKind.False =>
                    "Non",

                _ => null
            };

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        builder.AppendLine(
            $"• {label} : {value}");

        return true;
    }

    private static bool AppendEvidenceCollection(
        StringBuilder builder,
        JsonElement item,
        string propertyName,
        string label)
    {
        if (item.ValueKind != JsonValueKind.Object ||
            !item.TryGetProperty(
                propertyName,
                out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var values = property
            .EnumerateArray()
            .Select(GetEvidenceCollectionItem)
            .Where(value =>
                !string.IsNullOrWhiteSpace(value))
            .ToArray();

        if (values.Length == 0)
        {
            return false;
        }

        builder.AppendLine(
            $"• {label} : {string.Join(", ", values)}");

        return true;
    }

    private static string? GetEvidenceCollectionItem(
        JsonElement item)
    {
        if (item.ValueKind == JsonValueKind.String)
        {
            return item.GetString();
        }

        if (item.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        string? name = null;

        foreach (var propertyName in new[]
                 {
                 "name",
                 "displayName",
                 "roleName",
                 "userName"
             })
        {
            if (item.TryGetProperty(
                    propertyName,
                    out var property) &&
                property.ValueKind == JsonValueKind.String)
            {
                name = property.GetString();
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (item.TryGetProperty(
                "membershipType",
                out var membershipType) &&
            membershipType.ValueKind ==
            JsonValueKind.String)
        {
            return
                $"{name} ({membershipType.GetString()})";
        }

        return name;
    }

    private void ComposeFooter(
        IContainer container)
    {
        container
            .BorderTop(1)
            .BorderColor("#DCE3EA")
            .PaddingTop(8)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text(
                        $"Généré le " +
                        $"{FormatDate(_generatedAt)} UTC")
                    .FontSize(8)
                    .FontColor(MutedColor);

                row.RelativeItem()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(
                            style => style
                                .FontSize(8)
                                .FontColor(MutedColor));

                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
    }

    private static void AddInformationRow(
        TableDescriptor table,
        string label,
        string value)
    {
        table.Cell()
            .PaddingVertical(5)
            .Text(label)
            .SemiBold()
            .FontColor(MutedColor);

        table.Cell()
            .PaddingVertical(5)
            .Text(value);
    }

    private static void AddMetric(
        TableDescriptor table,
        string value,
        string label,
        string color)
    {
        table.Cell()
            .Padding(4)
            .Element(container =>
            {
                container
                    .Border(1)
                    .BorderColor("#DCE3EA")
                    .Background(Colors.White)
                    .Padding(12)
                    .AlignCenter()
                    .Column(column =>
                    {
                        column.Item()
                            .AlignCenter()
                            .Text(value)
                            .Bold()
                            .FontSize(17)
                            .FontColor(color);

                        column.Item()
                            .PaddingTop(3)
                            .AlignCenter()
                            .Text(label)
                            .FontSize(8)
                            .FontColor(MutedColor);
                    });
            });
    }

    private static string FormatAuditStatus(
        AuditStatus status)
    {
        return status switch
        {
            AuditStatus.Pending => "En attente",
            AuditStatus.Running => "En cours",
            AuditStatus.Completed => "Terminé",
            AuditStatus.CompletedWithWarnings =>
                "Terminé avec avertissements",
            AuditStatus.Failed => "Échec",
            _ => status.ToString()
        };
    }

    private static string FormatSeverity(
    RuleSeverity severity)
    {
        return severity switch
        {
            RuleSeverity.Low => "Faible",
            RuleSeverity.Medium => "Moyenne",
            RuleSeverity.High => "Élevée",
            RuleSeverity.Critical => "Critique",
            _ => severity.ToString()
        };
    }

    private static string FormatEvaluationStatus(
        RuleEvaluationStatus status)
    {
        return status switch
        {
            RuleEvaluationStatus.Compliant =>
                "Conforme",

            RuleEvaluationStatus.NonCompliant =>
                "Non conforme",

            RuleEvaluationStatus.NotApplicable =>
                "Non applicable",

            RuleEvaluationStatus.NotVerifiable =>
                "Non vérifiable",

            RuleEvaluationStatus.Error =>
                "Erreur",

            _ => status.ToString()
        };
    }

    private static string GetEvaluationStatusColor(
        RuleEvaluationStatus status)
    {
        return status switch
        {
            RuleEvaluationStatus.Compliant =>
                SuccessColor,

            RuleEvaluationStatus.NonCompliant =>
                DangerColor,

            RuleEvaluationStatus.NotApplicable =>
                MutedColor,

            RuleEvaluationStatus.NotVerifiable =>
                WarningColor,

            RuleEvaluationStatus.Error =>
                DangerColor,

            _ => TextColor
        };
    }

    private static string FormatDate(
        DateTimeOffset? date)
    {
        return date?.ToUniversalTime().ToString(
                   "dd/MM/yyyy HH:mm",
                   FrenchCulture)
               ?? "Non renseigné";
    }

    private static string FormatScore(
        decimal? score)
    {
        return score.HasValue
            ? $"{score.Value.ToString("0.00", FrenchCulture)} %"
            : "Non calculé";
    }

    private static IContainer StyleHeaderCell(
    IContainer container)
    {
        return container
            .Background(PrimaryColor)
            .PaddingVertical(7)
            .PaddingHorizontal(5);
    }

    private static IContainer StyleBodyCell(
        IContainer container,
        string backgroundColor)
    {
        return container
            .Background(backgroundColor)
            .BorderBottom(1)
            .BorderColor("#DCE3EA")
            .PaddingVertical(7)
            .PaddingHorizontal(5);
    }
}