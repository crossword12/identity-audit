import { useEffect, useState } from "react";
import {
  Gauge,
  RefreshCw,
  ShieldAlert,
  TriangleAlert,
  Users,
} from "lucide-react";
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

  const latestAudit = audits[0];
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

      {!loading && !error && !latestAudit && (
        <div className="dashboard-message">
          Aucun audit disponible pour cette cible.
        </div>
      )}

      {!loading && !error && latestAudit && (
        <>
          <div className="metrics-grid">
            <article className="metric-card">
              <div className="metric-icon blue">
                <Users size={23} />
              </div>
              <div>
                <span>Identités</span>
                <strong>{latestAudit.totalIdentities}</strong>
                <small>
                  {latestAudit.enabledIdentities} actives /{" "}
                  {latestAudit.disabledIdentities} inactives
                </small>
              </div>
            </article>

            <article className="metric-card">
              <div className="metric-icon orange">
                <ShieldAlert size={23} />
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
                <TriangleAlert size={23} />
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
                <Gauge size={23} />
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

          <div className="dashboard-panels">
            <section className="dashboard-panel">
              <div className="panel-heading">
                <div>
                  <h2>Synthèse des identités</h2>
                  <p>Dernier audit terminé</p>
                </div>
                <span className="completed-badge">
                  {latestAudit.auditStatus}
                </span>
              </div>

              <div className="summary-list">
                <div>
                  <span>Identités actives</span>
                  <strong>{latestAudit.enabledIdentities}</strong>
                </div>
                <div>
                  <span>Identités désactivées</span>
                  <strong>{latestAudit.disabledIdentities}</strong>
                </div>
                <div>
                  <span>Comptes privilégiés</span>
                  <strong>{latestAudit.privilegedIdentities}</strong>
                </div>
                <div>
                  <span>Comptes de service</span>
                  <strong>{latestAudit.serviceAccounts}</strong>
                </div>
                <div>
                  <span>Comptes invités</span>
                  <strong>{latestAudit.guestAccounts}</strong>
                </div>
                <div>
                  <span>Comptes verrouillés</span>
                  <strong>{latestAudit.lockedAccounts}</strong>
                </div>
              </div>
            </section>

            <section className="dashboard-panel">
              <div className="panel-heading">
                <div>
                  <h2>Résultats CIS</h2>
                  <p>Répartition des constats par gravité</p>
                </div>
              </div>

              <div className="severity-list">
                <div>
                  <span className="severity-label critical">Critique</span>
                  <strong>{latestAudit.criticalFindings}</strong>
                </div>
                <div>
                  <span className="severity-label high">Élevée</span>
                  <strong>{latestAudit.highFindings}</strong>
                </div>
                <div>
                  <span className="severity-label medium">Moyenne</span>
                  <strong>{latestAudit.mediumFindings}</strong>
                </div>
                <div>
                  <span className="severity-label low">Faible</span>
                  <strong>{latestAudit.lowFindings}</strong>
                </div>
              </div>

              <div className="cis-controls">
                <div>
                  <span>CIS Control 5</span>
                  <strong>{latestAudit.cisControl5Findings} constats</strong>
                </div>
                <div>
                  <span>CIS Control 6</span>
                  <strong>{latestAudit.cisControl6Findings} constats</strong>
                </div>
              </div>
            </section>
          </div>

          <section className="dashboard-panel audits-panel">
            <div className="panel-heading">
              <div>
                <h2>Audits récents</h2>
                <p>Historique de la cible sélectionnée</p>
              </div>
            </div>

            <div className="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Statut</th>
                    <th>Identités</th>
                    <th>Règles évaluées</th>
                    <th>Non conformes</th>
                    <th>Score CIS</th>
                  </tr>
                </thead>
                <tbody>
                  {audits.slice(0, 5).map((audit) => (
                    <tr key={audit.auditId}>
                      <td>{formatDate(audit.createdAt)}</td>
                      <td>
                        <span className="completed-badge">
                          {audit.auditStatus}
                        </span>
                      </td>
                      <td>{audit.totalIdentities}</td>
                      <td>{audit.evaluatedRules}</td>
                      <td>{audit.nonCompliantRules}</td>
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
