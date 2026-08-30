import { useEffect, useMemo, useState } from "react";
import {
  AlertCircle,
  LoaderCircle,
  RefreshCw,
  Search,
  ScrollText,
} from "lucide-react";
import { Link } from "react-router-dom";
import { getGlobalAuditLogs } from "../../api/auditLogsApi";
import type { AuditLog, AuditLogLevel } from "../../types/auditLog";
import "./ActivityLogsPage.css";

type LevelFilter = "All" | AuditLogLevel;

function formatDate(date: string): string {
  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "medium",
    timeStyle: "medium",
  }).format(new Date(date));
}

function getEventLabel(eventType: string): string {
  switch (eventType) {
    case "AuditCreated":
      return "Audit créé";
    case "AuditStarted":
      return "Audit démarré";
    case "AuditCompleted":
      return "Audit terminé";
    case "AuditRulesEvaluated":
      return "Règles CIS évaluées";
    case "AuditExported":
      return "Résultats exportés";
    default:
      return eventType;
  }
}

function getLevelLabel(level: AuditLogLevel): string {
  switch (level) {
    case "Information":
      return "Information";
    case "Warning":
      return "Avertissement";
    case "Error":
      return "Erreur";
  }
}

function ActivityLogsPage() {
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([]);

  const [loading, setLoading] = useState(true);

  const [error, setError] = useState("");

  const [search, setSearch] = useState("");

  const [levelFilter, setLevelFilter] = useState<LevelFilter>("All");

  async function loadActivityLogs() {
    try {
      setLoading(true);
      setError("");

      const result = await getGlobalAuditLogs(200);

      setAuditLogs(result);
    } catch {
      setError("Impossible de récupérer le journal d’activité.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadActivityLogs();
  }, []);

  const filteredLogs = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return auditLogs.filter((auditLog) => {
      const matchesLevel =
        levelFilter === "All" || auditLog.level === levelFilter;

      const searchableContent = [
        auditLog.eventType,
        getEventLabel(auditLog.eventType),
        auditLog.message,
        auditLog.applicationUserDisplayName,
        auditLog.applicationUserEmail,
        auditLog.auditId,
      ]
        .filter(Boolean)
        .join(" ")
        .toLowerCase();

      const matchesSearch =
        normalizedSearch.length === 0 ||
        searchableContent.includes(normalizedSearch);

      return matchesLevel && matchesSearch;
    });
  }, [auditLogs, levelFilter, search]);

  return (
    <div className="activity-logs-page">
      <div className="activity-logs-header">
        <div>
          <span className="activity-logs-section">Administration</span>

          <h1>Journal d’activité</h1>

          <p>
            Événements importants réalisés dans l’ensemble de l’application.
          </p>
        </div>

        <button
          type="button"
          className="activity-logs-refresh"
          onClick={() => void loadActivityLogs()}
          disabled={loading}
        >
          <RefreshCw
            size={18}
            className={loading ? "activity-logs-spin" : undefined}
          />
          Actualiser
        </button>
      </div>

      <section className="activity-logs-panel">
        <div className="activity-logs-toolbar">
          <div className="activity-logs-search">
            <Search size={18} />

            <input
              type="search"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Rechercher un événement, un utilisateur ou un audit"
              aria-label="Rechercher dans le journal"
            />
          </div>

          <select
            value={levelFilter}
            onChange={(event) =>
              setLevelFilter(event.target.value as LevelFilter)
            }
            aria-label="Filtrer par niveau"
          >
            <option value="All">Tous les niveaux</option>

            <option value="Information">Informations</option>

            <option value="Warning">Avertissements</option>

            <option value="Error">Erreurs</option>
          </select>

          {!loading && !error && (
            <span className="activity-logs-count">
              {filteredLogs.length} sur {auditLogs.length} événement(s)
            </span>
          )}
        </div>

        {loading && (
          <div className="activity-logs-state">
            <LoaderCircle className="activity-logs-spin" size={25} />
            Chargement du journal...
          </div>
        )}

        {error && (
          <div className="activity-logs-state error">
            <AlertCircle size={22} />
            {error}
          </div>
        )}

        {!loading && !error && filteredLogs.length === 0 && (
          <div className="activity-logs-empty">
            <ScrollText size={36} />

            <strong>Aucun événement trouvé</strong>

            <span>
              Modifiez les filtres ou effectuez une nouvelle opération.
            </span>
          </div>
        )}

        {!loading && !error && filteredLogs.length > 0 && (
          <div className="activity-logs-table-container">
            <table className="activity-logs-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Événement</th>
                  <th>Niveau</th>
                  <th>Utilisateur</th>
                  <th>Audit</th>
                  <th>Message</th>
                </tr>
              </thead>

              <tbody>
                {filteredLogs.map((auditLog) => (
                  <tr key={auditLog.id}>
                    <td className="activity-log-date">
                      {formatDate(auditLog.createdAt)}
                    </td>

                    <td>
                      <strong>{getEventLabel(auditLog.eventType)}</strong>

                      <span className="activity-log-code">
                        {auditLog.eventType}
                      </span>
                    </td>

                    <td>
                      <span
                        className={`activity-log-level ${auditLog.level.toLowerCase()}`}
                      >
                        {getLevelLabel(auditLog.level)}
                      </span>
                    </td>

                    <td>
                      <strong>
                        {auditLog.applicationUserDisplayName ?? "Système"}
                      </strong>

                      {auditLog.applicationUserEmail && (
                        <span className="activity-log-secondary">
                          {auditLog.applicationUserEmail}
                        </span>
                      )}
                    </td>

                    <td>
                      {auditLog.auditId ? (
                        <Link
                          to={`/audits/${auditLog.auditId}`}
                          className="activity-log-audit-link"
                        >
                          #{auditLog.auditId.slice(0, 8)}
                        </Link>
                      ) : (
                        <span className="activity-log-secondary">
                          Application
                        </span>
                      )}
                    </td>

                    <td className="activity-log-message">{auditLog.message}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}

export default ActivityLogsPage;
