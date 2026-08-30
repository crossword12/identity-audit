import axios from "axios";
import { useEffect, useMemo, useState } from "react";
import {
  AlertTriangle,
  ArrowLeft,
  CheckCircle2,
  Download,
  FileSearch,
  KeyRound,
  LoaderCircle,
  Network,
  RefreshCw,
  ShieldCheck,
  Users,
  XCircle,
  ScrollText,
} from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";
import { exportAuditCsv, getAuditById } from "../../api/auditsApi";
import {
  evaluateAuditRules,
  getAuditRuleEvaluations,
} from "../../api/ruleEvaluationsApi";
import AuditGroupsPanel from "../../components/audit-details/AuditGroupsPanel";
import AuditIdentitiesPanel from "../../components/audit-details/AuditIdentitiesPanel";
import AuditRolesPanel from "../../components/audit-details/AuditRolesPanel";
import type { Audit, AuditStatus } from "../../types/audit";
import type {
  RuleEvaluation,
  RuleEvaluationStatus,
  RuleSeverity,
} from "../../types/ruleEvaluation";
import AuditLogsPanel from "../../components/audit-details/AuditLogsPanel";
import "./AuditDetailsPage.css";

type DetailTab = "results" | "identities" | "groups" | "roles" | "logs";
type EvaluationFilter = "All" | RuleEvaluationStatus;
type EvidenceRecord = Record<string, unknown>;

function formatDate(date: string | null): string {
  if (!date) {
    return "—";
  }

  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(date));
}

function formatScore(score: number | null): string {
  if (score === null) {
    return "Non évalué";
  }

  return `${score.toLocaleString("fr-FR", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })} %`;
}

function getAuditStatusLabel(status: AuditStatus): string {
  switch (status) {
    case "Pending":
      return "En attente";
    case "Running":
      return "En cours";
    case "Completed":
      return "Terminé";
    case "CompletedWithWarnings":
      return "Terminé avec avertissements";
    case "Failed":
      return "Échec";
  }
}

function getEvaluationStatusLabel(status: RuleEvaluationStatus): string {
  switch (status) {
    case "Compliant":
      return "Conforme";
    case "NonCompliant":
      return "Non conforme";
    case "NotApplicable":
      return "Non applicable";
    case "NotVerifiable":
      return "Non vérifiable";
    case "Error":
      return "Erreur";
  }
}

function getSeverityLabel(severity: RuleSeverity): string {
  switch (severity) {
    case "Low":
      return "Faible";
    case "Medium":
      return "Moyenne";
    case "High":
      return "Élevée";
    case "Critical":
      return "Critique";
  }
}

function isEvidenceRecord(value: unknown): value is EvidenceRecord {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function parseEvidence(evidenceJson: string | null): EvidenceRecord[] {
  if (!evidenceJson) {
    return [];
  }

  try {
    const parsed = JSON.parse(evidenceJson) as unknown;

    if (Array.isArray(parsed)) {
      return parsed.filter(isEvidenceRecord);
    }

    if (isEvidenceRecord(parsed)) {
      return [parsed];
    }

    return [];
  } catch {
    return [];
  }
}

function getEvidenceLabel(key: string): string {
  const labels: Record<string, string> = {
    id: "Identifiant interne",
    reason: "Raison",
    displayName: "Nom affiché",
    userName: "Nom d’utilisateur",
    externalId: "Identifiant externe",
    owner: "Propriétaire",
    description: "Description",
    groupName: "Groupe",
    membershipType: "Type d’appartenance",
    groupExternalId: "Identifiant du groupe",
    privilegedGroups: "Groupes privilégiés",
    privilegedRoles: "Rôles privilégiés",
  };

  return labels[key] ?? key;
}

function formatEvidenceValue(value: unknown): string {
  if (value === null || value === undefined || value === "") {
    return "Non renseigné";
  }

  if (Array.isArray(value)) {
    if (value.length === 0) {
      return "Aucun";
    }

    return value
      .map((item) => {
        if (!isEvidenceRecord(item)) {
          return String(item);
        }

        const name =
          item.name ??
          item.displayName ??
          item.groupName ??
          item.roleName ??
          item.externalId;

        const membership = item.membershipType
          ? ` (${String(item.membershipType)})`
          : "";

        return name ? `${String(name)}${membership}` : JSON.stringify(item);
      })
      .join(", ");
  }

  if (isEvidenceRecord(value)) {
    return JSON.stringify(value);
  }

  return String(value);
}

function getEvidenceTitle(record: EvidenceRecord, index: number): string {
  const identity =
    record.displayName ??
    record.userName ??
    record.groupName ??
    record.externalId;

  return identity
    ? `Constat ${index + 1} — ${String(identity)}`
    : `Constat ${index + 1}`;
}

function AuditDetailsPage() {
  const navigate = useNavigate();
  const { auditId } = useParams<{ auditId: string }>();

  const [audit, setAudit] = useState<Audit | null>(null);
  const [evaluations, setEvaluations] = useState<RuleEvaluation[]>([]);
  const [activeTab, setActiveTab] = useState<DetailTab>("results");
  const [filter, setFilter] = useState<EvaluationFilter>("All");
  const [loading, setLoading] = useState(true);
  const [evaluating, setEvaluating] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [error, setError] = useState("");
  const [evaluationError, setEvaluationError] = useState("");
  const [exportError, setExportError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function loadAuditDetails() {
      if (!auditId) {
        setError("Identifiant d’audit manquant.");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError("");

        const [auditResult, evaluationResult] = await Promise.all([
          getAuditById(auditId),
          getAuditRuleEvaluations(auditId),
        ]);

        if (!cancelled) {
          setAudit(auditResult);
          setEvaluations(evaluationResult);
        }
      } catch {
        if (!cancelled) {
          setError("Impossible de récupérer les détails de l’audit.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadAuditDetails();

    return () => {
      cancelled = true;
    };
  }, [auditId]);

  const filteredEvaluations = useMemo(() => {
    if (filter === "All") {
      return evaluations;
    }

    return evaluations.filter((evaluation) => evaluation.status === filter);
  }, [evaluations, filter]);

  const compliantCount = evaluations.filter(
    (evaluation) => evaluation.status === "Compliant",
  ).length;

  const nonCompliantCount = evaluations.filter(
    (evaluation) => evaluation.status === "NonCompliant",
  ).length;

  const findingCount = evaluations.reduce(
    (total, evaluation) => total + evaluation.findingCount,
    0,
  );

  async function handleExportCsv() {
    if (!audit) {
      return;
    }

    try {
      setExporting(true);
      setExportError("");

      const result = await exportAuditCsv(audit.id);
      const downloadUrl = URL.createObjectURL(result.content);
      const link = document.createElement("a");

      try {
        link.href = downloadUrl;
        link.download = result.fileName;

        document.body.appendChild(link);
        link.click();
      } finally {
        link.remove();

        window.setTimeout(() => {
          URL.revokeObjectURL(downloadUrl);
        }, 1000);
      }
    } catch {
      setExportError("Impossible d’exporter les résultats de l’audit.");
    } finally {
      setExporting(false);
    }
  }

  async function handleEvaluateRules() {
    if (!audit) {
      return;
    }

    try {
      setEvaluating(true);
      setEvaluationError("");

      const result = await evaluateAuditRules(audit.id);

      setEvaluations(result.evaluations);

      setAudit((currentAudit) =>
        currentAudit
          ? {
              ...currentAudit,
              complianceScore: result.complianceScore,
            }
          : currentAudit,
      );
    } catch (requestError) {
      let message = "L’évaluation des règles a échoué.";

      if (axios.isAxiosError(requestError)) {
        const response = requestError.response?.data as
          | {
              detail?: string;
              message?: string;
              title?: string;
            }
          | undefined;

        message =
          response?.detail ?? response?.message ?? response?.title ?? message;
      }

      setEvaluationError(message);
    } finally {
      setEvaluating(false);
    }
  }

  if (loading) {
    return (
      <div className="audit-details-message">
        Chargement du détail de l’audit...
      </div>
    );
  }

  if (error || !audit) {
    return (
      <div className="audit-details-page">
        <button
          className="back-to-audits"
          type="button"
          onClick={() => navigate("/audits")}
        >
          <ArrowLeft size={18} />
          Retour aux audits
        </button>

        <div className="audit-details-message error">
          {error || "Audit introuvable."}
        </div>
      </div>
    );
  }

  return (
    <div className="audit-details-page">
      <button
        className="back-to-audits"
        type="button"
        onClick={() => navigate("/audits")}
      >
        <ArrowLeft size={18} />
        Retour aux audits
      </button>

      <div className="audit-details-header">
        <div>
          <div className="audit-details-title-line">
            <h1>Détail de l’audit</h1>

            <span
              className={`audit-detail-status ${audit.status.toLowerCase()}`}
            >
              {getAuditStatusLabel(audit.status)}
            </span>
          </div>

          <p>
            Référence #{audit.id.slice(0, 8)} · {audit.targetName}
          </p>
        </div>

        {activeTab === "results" &&
          (evaluations.length > 0 || audit.status === "Running") && (
            <div className="audit-details-actions">
              {evaluations.length > 0 && (
                <button
                  className="export-csv-button"
                  type="button"
                  disabled={exporting}
                  aria-busy={exporting}
                  onClick={() => void handleExportCsv()}
                >
                  {exporting ? (
                    <LoaderCircle className="audit-detail-spin" size={18} />
                  ) : (
                    <Download size={18} />
                  )}

                  {exporting ? "Exportation..." : "Exporter en CSV"}
                </button>
              )}

              {audit.status === "Running" && (
                <button
                  className="evaluate-rules-button"
                  type="button"
                  disabled={evaluating}
                  onClick={() => void handleEvaluateRules()}
                >
                  {evaluating ? (
                    <LoaderCircle className="audit-detail-spin" size={18} />
                  ) : (
                    <RefreshCw size={18} />
                  )}

                  {evaluating ? "Évaluation..." : "Évaluer les règles"}
                </button>
              )}
            </div>
          )}
      </div>

      <section className="audit-overview">
        <div>
          <span>Cible</span>
          <strong>{audit.targetName}</strong>
        </div>

        <div>
          <span>Démarré le</span>
          <strong>{formatDate(audit.startedAt)}</strong>
        </div>

        <div>
          <span>Terminé le</span>
          <strong>{formatDate(audit.completedAt)}</strong>
        </div>

        <div className="audit-overview-score">
          <span>Score de conformité</span>
          <strong>{formatScore(audit.complianceScore)}</strong>
        </div>
      </section>

      <div
        className="audit-detail-tabs"
        role="tablist"
        aria-label="Contenu du détail de l’audit"
      >
        <button
          className={`audit-detail-tab ${
            activeTab === "results" ? "active" : ""
          }`}
          type="button"
          role="tab"
          aria-selected={activeTab === "results"}
          onClick={() => setActiveTab("results")}
        >
          <FileSearch size={18} />
          Résultats CIS
        </button>

        <button
          className={`audit-detail-tab ${
            activeTab === "identities" ? "active" : ""
          }`}
          type="button"
          role="tab"
          aria-selected={activeTab === "identities"}
          onClick={() => setActiveTab("identities")}
        >
          <Users size={18} />
          Identités
        </button>

        <button
          className={`audit-detail-tab ${
            activeTab === "groups" ? "active" : ""
          }`}
          type="button"
          role="tab"
          aria-selected={activeTab === "groups"}
          onClick={() => setActiveTab("groups")}
        >
          <Network size={18} />
          Groupes
        </button>

        <button
          className={`audit-detail-tab ${
            activeTab === "roles" ? "active" : ""
          }`}
          type="button"
          role="tab"
          aria-selected={activeTab === "roles"}
          onClick={() => setActiveTab("roles")}
        >
          <KeyRound size={18} />
          Rôles
        </button>

        <button
          className={`audit-detail-tab ${activeTab === "logs" ? "active" : ""}`}
          type="button"
          role="tab"
          aria-selected={activeTab === "logs"}
          onClick={() => setActiveTab("logs")}
        >
          <ScrollText size={18} />
          Journal
        </button>
      </div>

      {activeTab === "results" && (
        <>
          <div className="evaluation-summary-grid">
            <article>
              <div className="evaluation-summary-icon total">
                <FileSearch size={21} />
              </div>

              <div>
                <span>Règles évaluées</span>
                <strong>{evaluations.length}</strong>
              </div>
            </article>

            <article>
              <div className="evaluation-summary-icon compliant">
                <CheckCircle2 size={21} />
              </div>

              <div>
                <span>Conformes</span>
                <strong>{compliantCount}</strong>
              </div>
            </article>

            <article>
              <div className="evaluation-summary-icon noncompliant">
                <XCircle size={21} />
              </div>

              <div>
                <span>Non conformes</span>
                <strong>{nonCompliantCount}</strong>
              </div>
            </article>

            <article>
              <div className="evaluation-summary-icon findings">
                <AlertTriangle size={21} />
              </div>

              <div>
                <span>Constats détectés</span>
                <strong>{findingCount}</strong>
              </div>
            </article>
          </div>

          {(evaluationError || exportError) && (
            <div className="evaluation-error">
              <XCircle size={18} />
              {evaluationError || exportError}
            </div>
          )}

          <section className="evaluations-section">
            <div className="evaluations-toolbar">
              <div>
                <h2>Résultats des règles CIS</h2>
                <span>{filteredEvaluations.length} résultat(s)</span>
              </div>

              <label>
                <span>Statut</span>

                <select
                  value={filter}
                  onChange={(event) =>
                    setFilter(event.target.value as EvaluationFilter)
                  }
                >
                  <option value="All">Tous les statuts</option>
                  <option value="Compliant">Conformes</option>
                  <option value="NonCompliant">Non conformes</option>
                  <option value="NotApplicable">Non applicables</option>
                  <option value="NotVerifiable">Non vérifiables</option>
                  <option value="Error">Erreurs</option>
                </select>
              </label>
            </div>

            {filteredEvaluations.length === 0 ? (
              <div className="evaluations-empty">
                <ShieldCheck size={34} />

                <strong>Aucun résultat disponible</strong>

                <span>
                  Les règles n’ont pas encore été évaluées ou aucun résultat ne
                  correspond au filtre.
                </span>
              </div>
            ) : (
              <div className="evaluation-list">
                {filteredEvaluations.map((evaluation) => {
                  const evidenceItems = parseEvidence(evaluation.evidenceJson);

                  return (
                    <article className="evaluation-card" key={evaluation.id}>
                      <div className="evaluation-card-header">
                        <div>
                          <div className="evaluation-identifiers">
                            <span className="rule-code">
                              {evaluation.ruleCode}
                            </span>

                            <span>{evaluation.cisControl}</span>

                            <span>
                              {evaluation.targetType === "EntraId"
                                ? "Microsoft Entra ID"
                                : "Active Directory"}
                            </span>
                          </div>

                          <h3>{evaluation.ruleName}</h3>
                        </div>

                        <div className="evaluation-badges">
                          <span
                            className={`severity-badge ${evaluation.severity.toLowerCase()}`}
                          >
                            Sévérité {getSeverityLabel(evaluation.severity)}
                          </span>

                          <span
                            className={`evaluation-status ${evaluation.status.toLowerCase()}`}
                          >
                            {evaluation.status === "Compliant" ? (
                              <CheckCircle2 size={15} />
                            ) : evaluation.status === "NonCompliant" ? (
                              <XCircle size={15} />
                            ) : (
                              <AlertTriangle size={15} />
                            )}

                            {getEvaluationStatusLabel(evaluation.status)}
                          </span>
                        </div>
                      </div>

                      <div className="evaluation-card-meta">
                        <span>
                          <strong>{evaluation.findingCount}</strong> constat(s)
                        </span>

                        <span>
                          Évaluée le {formatDate(evaluation.evaluatedAt)}
                        </span>
                      </div>

                      {evaluation.errorMessage && (
                        <div className="rule-error-message">
                          {evaluation.errorMessage}
                        </div>
                      )}

                      {evaluation.recommendation && (
                        <div className="recommendation-block">
                          <strong>Recommandation</strong>
                          <p>{evaluation.recommendation}</p>
                        </div>
                      )}

                      {evaluation.evidenceJson && (
                        <details className="evidence-details">
                          <summary>
                            Voir les preuves ({evaluation.findingCount})
                          </summary>

                          {evidenceItems.length > 0 ? (
                            <div className="evidence-list">
                              {evidenceItems.map((record, index) => (
                                <article
                                  className="evidence-card"
                                  key={`${evaluation.id}-${index}`}
                                >
                                  <h4>{getEvidenceTitle(record, index)}</h4>

                                  <dl>
                                    {Object.entries(record).map(
                                      ([key, value]) => (
                                        <div key={key}>
                                          <dt>{getEvidenceLabel(key)}</dt>

                                          <dd>{formatEvidenceValue(value)}</dd>
                                        </div>
                                      ),
                                    )}
                                  </dl>
                                </article>
                              ))}
                            </div>
                          ) : (
                            <pre className="raw-evidence">
                              {evaluation.evidenceJson}
                            </pre>
                          )}
                        </details>
                      )}
                    </article>
                  );
                })}
              </div>
            )}
          </section>
        </>
      )}

      {activeTab === "identities" && (
        <AuditIdentitiesPanel auditId={audit.id} />
      )}

      {activeTab === "groups" && <AuditGroupsPanel auditId={audit.id} />}

      {activeTab === "roles" && <AuditRolesPanel auditId={audit.id} />}

      {activeTab === "logs" && <AuditLogsPanel auditId={audit.id} />}
    </div>
  );
}

export default AuditDetailsPage;
