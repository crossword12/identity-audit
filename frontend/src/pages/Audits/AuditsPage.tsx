import axios from "axios";
import { useEffect, useMemo, useState } from "react";
import {
  AlertTriangle,
  CheckCircle2,
  ClipboardList,
  Clock3,
  Eye,
  Flag,
  LoaderCircle,
  Play,
  Plus,
  RefreshCw,
  XCircle,
} from "lucide-react";
import { Link } from "react-router-dom";
import { completeAudit, getAudits, startAudit } from "../../api/auditsApi";
import CreateAuditModal from "../../components/audits/CreateAuditModal";
import type { Audit, AuditStatus } from "../../types/audit";
import "./AuditsPage.css";

type StatusFilter = "All" | AuditStatus;

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

function getStatusLabel(status: AuditStatus): string {
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

function getStatusIcon(status: AuditStatus) {
  switch (status) {
    case "Pending":
      return <Clock3 size={15} />;
    case "Running":
      return <LoaderCircle className="audit-spin" size={15} />;
    case "Completed":
      return <CheckCircle2 size={15} />;
    case "CompletedWithWarnings":
      return <AlertTriangle size={15} />;
    case "Failed":
      return <XCircle size={15} />;
  }
}

function AuditsPage() {
  const [audits, setAudits] = useState<Audit[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [actionError, setActionError] = useState("");
  const [actionId, setActionId] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("All");
  const [refreshKey, setRefreshKey] = useState(0);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadAudits() {
      try {
        setLoading(true);
        setError("");

        const result = await getAudits();

        if (!cancelled) {
          setAudits(result);
        }
      } catch {
        if (!cancelled) {
          setError("Impossible de récupérer les audits.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadAudits();

    return () => {
      cancelled = true;
    };
  }, [refreshKey]);

  function handleAuditCreated(createdAudit: Audit) {
    setAudits((currentAudits) => [createdAudit, ...currentAudits]);

    setIsCreateModalOpen(false);
  }

  const filteredAudits = useMemo(() => {
    if (statusFilter === "All") {
      return audits;
    }

    return audits.filter((audit) => audit.status === statusFilter);
  }, [audits, statusFilter]);

  const runningCount = audits.filter(
    (audit) => audit.status === "Running",
  ).length;

  const completedCount = audits.filter(
    (audit) =>
      audit.status === "Completed" || audit.status === "CompletedWithWarnings",
  ).length;

  const attentionCount = audits.filter(
    (audit) =>
      audit.status === "Failed" || audit.status === "CompletedWithWarnings",
  ).length;

  async function handleAuditAction(audit: Audit) {
    if (audit.status !== "Pending" && audit.status !== "Running") {
      return;
    }

    try {
      setActionId(audit.id);
      setActionError("");

      const updatedAudit =
        audit.status === "Pending"
          ? await startAudit(audit.id)
          : await completeAudit(audit.id);

      setAudits((currentAudits) =>
        currentAudits.map((currentAudit) =>
          currentAudit.id === updatedAudit.id ? updatedAudit : currentAudit,
        ),
      );
    } catch (requestError) {
      let message = "L’action sur l’audit a échoué.";

      if (axios.isAxiosError(requestError)) {
        const response = requestError.response?.data as
          | { message?: string; title?: string }
          | undefined;

        message = response?.message ?? response?.title ?? message;
      }

      setActionError(message);
    } finally {
      setActionId(null);
    }
  }

  return (
    <div className="audits-page">
      <div className="audits-header">
        <div>
          <h1>Audits</h1>
          <p>
            Suivi des audits d’identités et de conformité CIS Controls 5 et 6.
          </p>
        </div>

        <div className="audits-header-actions">
          <div className="audits-count">
            <ClipboardList size={18} />
            {audits.length} audit(s)
          </div>

          <button
            className="refresh-audits-button"
            type="button"
            disabled={loading}
            onClick={() => setRefreshKey((value) => value + 1)}
          >
            <RefreshCw className={loading ? "audit-spin" : ""} size={18} />
            Actualiser
          </button>

          <button
            className="new-audit-button"
            type="button"
            onClick={() => setIsCreateModalOpen(true)}
          >
            <Plus size={18} />
            Nouvel audit
          </button>
        </div>
      </div>

      <div className="audit-summary-grid">
        <article className="audit-summary-card">
          <div className="audit-summary-icon total">
            <ClipboardList size={21} />
          </div>

          <div>
            <span>Total des audits</span>
            <strong>{audits.length}</strong>
          </div>
        </article>

        <article className="audit-summary-card">
          <div className="audit-summary-icon running">
            <Clock3 size={21} />
          </div>

          <div>
            <span>En cours</span>
            <strong>{runningCount}</strong>
          </div>
        </article>

        <article className="audit-summary-card">
          <div className="audit-summary-icon completed">
            <CheckCircle2 size={21} />
          </div>

          <div>
            <span>Terminés</span>
            <strong>{completedCount}</strong>
          </div>
        </article>

        <article className="audit-summary-card">
          <div className="audit-summary-icon attention">
            <AlertTriangle size={21} />
          </div>

          <div>
            <span>À surveiller</span>
            <strong>{attentionCount}</strong>
          </div>
        </article>
      </div>

      {actionError && (
        <div className="audits-action-error">
          <XCircle size={18} />
          {actionError}
        </div>
      )}

      {loading && (
        <div className="audits-message">Chargement des audits...</div>
      )}

      {!loading && error && (
        <div className="audits-message audits-error">{error}</div>
      )}

      {!loading && !error && (
        <section className="audits-panel">
          <div className="audits-toolbar">
            <div>
              <h2>Historique des audits</h2>
              <span>{filteredAudits.length} résultat(s)</span>
            </div>

            <label className="audit-filter">
              <span>Statut</span>

              <select
                value={statusFilter}
                onChange={(event) =>
                  setStatusFilter(event.target.value as StatusFilter)
                }
              >
                <option value="All">Tous les statuts</option>
                <option value="Pending">En attente</option>
                <option value="Running">En cours</option>
                <option value="Completed">Terminés</option>
                <option value="CompletedWithWarnings">
                  Terminés avec avertissements
                </option>
                <option value="Failed">Échecs</option>
              </select>
            </label>
          </div>

          {filteredAudits.length === 0 ? (
            <div className="audits-empty">
              Aucun audit ne correspond au filtre sélectionné.
            </div>
          ) : (
            <div className="audits-table-container">
              <table className="audits-table">
                <thead>
                  <tr>
                    <th>Référence</th>
                    <th>Cible</th>
                    <th>Statut</th>
                    <th>Démarré le</th>
                    <th>Terminé le</th>
                    <th>Score CIS</th>
                    <th>Actions</th>
                  </tr>
                </thead>

                <tbody>
                  {filteredAudits.map((audit) => {
                    const isProcessing = actionId === audit.id;

                    return (
                      <tr key={audit.id}>
                        <td>
                          <span className="audit-reference">
                            #{audit.id.slice(0, 8)}
                          </span>

                          <small>{formatDate(audit.createdAt)}</small>
                        </td>

                        <td>
                          <strong className="audit-target-name">
                            {audit.targetName}
                          </strong>
                        </td>

                        <td>
                          <span
                            className={`audit-status ${audit.status.toLowerCase()}`}
                          >
                            {getStatusIcon(audit.status)}
                            {getStatusLabel(audit.status)}
                          </span>
                        </td>

                        <td>{formatDate(audit.startedAt)}</td>

                        <td>{formatDate(audit.completedAt)}</td>

                        <td>
                          <span
                            className={`audit-score ${
                              audit.complianceScore === null
                                ? "not-evaluated"
                                : ""
                            }`}
                          >
                            {formatScore(audit.complianceScore)}
                          </span>
                        </td>

                        <td>
                          <div className="audit-row-actions">
                            {audit.status === "Pending" && (
                              <button
                                className="audit-action-button start"
                                type="button"
                                disabled={actionId !== null}
                                onClick={() => void handleAuditAction(audit)}
                              >
                                {isProcessing ? (
                                  <LoaderCircle
                                    className="audit-spin"
                                    size={16}
                                  />
                                ) : (
                                  <Play size={16} />
                                )}
                                Démarrer
                              </button>
                            )}

                            {audit.status === "Running" && (
                              <button
                                className="audit-action-button complete"
                                type="button"
                                disabled={actionId !== null}
                                onClick={() => void handleAuditAction(audit)}
                              >
                                {isProcessing ? (
                                  <LoaderCircle
                                    className="audit-spin"
                                    size={16}
                                  />
                                ) : (
                                  <Flag size={16} />
                                )}
                                Terminer
                              </button>
                            )}

                            <Link
                              className="audit-details-link"
                              to={`/audits/${audit.id}`}
                            >
                              <Eye size={16} />
                              Détails
                            </Link>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </section>
      )}

      {isCreateModalOpen && (
        <CreateAuditModal
          onClose={() => setIsCreateModalOpen(false)}
          onCreated={handleAuditCreated}
        />
      )}
    </div>
  );
}

export default AuditsPage;
