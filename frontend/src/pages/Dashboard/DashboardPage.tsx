import { useEffect, useState } from "react";
import {
  Gauge,
  RefreshCw,
  ShieldAlert,
  TriangleAlert,
  Users,
} from "lucide-react";
import { Link } from "react-router-dom";
import { getTargetDashboard } from "../../api/dashboardApi";
import { getTargets } from "../../api/targetsApi";
import type { DashboardAudit } from "../../types/dashboard";
import type { Target } from "../../types/target";
import "./DashboardPage.css";

function formatDate(date: string | null): string {
  if (!date) {
    return "Non disponible";
  }

  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(date));
}

function getScoreClass(score: number | null): string {
  if (score === null) return "neutral";
  if (score >= 80) return "success";
  if (score >= 50) return "warning";
  return "danger";
}

function getAuditStatusLabel(status: string): string {
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
    default:
      return status;
  }
}

function DashboardPage() {
  const [targets, setTargets] = useState<Target[]>([]);
  const [selectedTargetId, setSelectedTargetId] = useState("");
  const [audits, setAudits] = useState<DashboardAudit[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshKey, setRefreshKey] = useState(0);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadTargets() {
      try {
        const result = await getTargets();
        setTargets(result);

        if (result.length > 0) {
          setSelectedTargetId(result[0].id);
        } else {
          setLoading(false);
        }
      } catch {
        setError("Impossible de récupérer les cibles.");
        setLoading(false);
      }
    }

    void loadTargets();
  }, []);

  useEffect(() => {
    if (!selectedTargetId) {
      return;
    }

    async function loadDashboard() {
      try {
        setLoading(true);
        setError("");

        const result = await getTargetDashboard(selectedTargetId);
        setAudits(result);
      } catch {
        setAudits([]);
        setError("Impossible de récupérer les données du tableau de bord.");
      } finally {
        setLoading(false);
      }
    }

    void loadDashboard();
  }, [selectedTargetId, refreshKey]);

  const latestAudit = audits.find(
    (audit) =>
      audit.auditStatus === "Completed" ||
      audit.auditStatus === "CompletedWithWarnings",
  );

  const score = latestAudit?.complianceScore ?? null;
  const scoreClass = getScoreClass(score);

  return (
    <div className="dashboard-page">
      <div className="dashboard-header">
        <div>
          <h1>Tableau de bord</h1>
          <p>Vue globale de la conformité des identités et des privilèges.</p>
        </div>

        <div className="dashboard-actions">
          <label htmlFor="target-select">Cible analysée</label>

          <div className="select-row">
            <select
              id="target-select"
              value={selectedTargetId}
              onChange={(event) => setSelectedTargetId(event.target.value)}
            >
              {targets.map((target) => (
                <option key={target.id} value={target.id}>
                  {target.name}
                </option>
              ))}
            </select>

            <button
              type="button"
              title="Actualiser"
              aria-label="Actualiser le tableau de bord"
              disabled={loading}
              onClick={() => setRefreshKey((value) => value + 1)}
            >
              <RefreshCw size={18} />
            </button>
          </div>
        </div>
      </div>

      {loading && (
        <div className="dashboard-message">Chargement des données...</div>
      )}

      {!loading && error && (
        <div className="dashboard-message error-message">{error}</div>
      )}

      {!loading && !error && audits.length === 0 && (
        <div className="dashboard-message">
          Aucun audit disponible pour cette cible.
        </div>
      )}

      {!loading && !error && audits.length > 0 && (
        <>
          {latestAudit ? (
            <>
              <div className="metrics-grid">
                <article className="metric-card">
                  <div className="metric-icon blue">
                    <Users size={22} />
                  </div>

                  <div>
                    <span>Identités</span>
                    <strong>{latestAudit.totalIdentities}</strong>
                    <small>
                      {latestAudit.enabledIdentities} actives ·{" "}
                      {latestAudit.disabledIdentities} inactives
                    </small>
                  </div>
                </article>

                <article className="metric-card">
                  <div className="metric-icon orange">
                    <ShieldAlert size={22} />
                  </div>

                  <div>
                    <span>Comptes privilégiés</span>
                    <strong>{latestAudit.privilegedIdentities}</strong>
                    <small>
                      {latestAudit.serviceAccounts} compte(s) de service
                    </small>
                  </div>
                </article>

                <article className="metric-card">
                  <div className="metric-icon red">
                    <TriangleAlert size={22} />
                  </div>

                  <div>
                    <span>Constats</span>
                    <strong>{latestAudit.totalFindings}</strong>
                    <small>
                      {latestAudit.nonCompliantRules} règle(s) non conforme(s)
                    </small>
                  </div>
                </article>

                <article className={`metric-card score-card ${scoreClass}`}>
                  <div className="metric-icon">
                    <Gauge size={22} />
                  </div>

                  <div>
                    <span>Score CIS</span>
                    <strong>
                      {score === null ? "N/A" : `${score.toFixed(2)} %`}
                    </strong>

                    <div className="score-progress">
                      <span style={{ width: `${score ?? 0}%` }} />
                    </div>
                  </div>
                </article>
              </div>

              <section className="dashboard-panel dashboard-cis-panel">
                <div className="dashboard-panel-heading">
                  <div>
                    <h2>Résultats CIS</h2>
                    <p>Répartition des constats du dernier audit terminé</p>
                  </div>

                  <span className="findings-total">
                    {latestAudit.totalFindings} constat(s)
                  </span>
                </div>

                <div className="severity-grid">
                  <article className="severity-card critical">
                    <span>Critique</span>
                    <strong>{latestAudit.criticalFindings}</strong>
                  </article>

                  <article className="severity-card high">
                    <span>Élevée</span>
                    <strong>{latestAudit.highFindings}</strong>
                  </article>

                  <article className="severity-card medium">
                    <span>Moyenne</span>
                    <strong>{latestAudit.mediumFindings}</strong>
                  </article>

                  <article className="severity-card low">
                    <span>Faible</span>
                    <strong>{latestAudit.lowFindings}</strong>
                  </article>
                </div>

                <div className="dashboard-cis-controls">
                  <div>
                    <span>CIS Control 5</span>
                    <strong>
                      {latestAudit.cisControl5Findings} constat(s)
                    </strong>
                  </div>

                  <div>
                    <span>CIS Control 6</span>
                    <strong>
                      {latestAudit.cisControl6Findings} constat(s)
                    </strong>
                  </div>
                </div>
              </section>
            </>
          ) : (
            <div className="dashboard-message">
              Aucun audit terminé n’est disponible pour cette cible.
            </div>
          )}

          <section className="dashboard-panel dashboard-audits-panel">
            <div className="dashboard-panel-heading">
              <div>
                <h2>Audits récents</h2>
                <p>Trois derniers audits de la cible sélectionnée</p>
              </div>

              <Link className="view-all-audits" to="/audits">
                Voir tous les audits →
              </Link>
            </div>

            <div className="dashboard-table-wrapper">
              <table className="dashboard-table">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Statut</th>
                    <th>Identités</th>
                    <th>Score CIS</th>
                  </tr>
                </thead>

                <tbody>
                  {audits.slice(0, 3).map((audit) => (
                    <tr key={audit.auditId}>
                      <td>{formatDate(audit.createdAt)}</td>

                      <td>
                        <span
                          className={`dashboard-audit-status ${audit.auditStatus.toLowerCase()}`}
                        >
                          {getAuditStatusLabel(audit.auditStatus)}
                        </span>
                      </td>

                      <td>{audit.totalIdentities}</td>

                      <td>
                        {audit.complianceScore === null
                          ? "Non évalué"
                          : `${audit.complianceScore.toFixed(2)} %`}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        </>
      )}
    </div>
  );
}

export default DashboardPage;
