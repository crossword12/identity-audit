import { useEffect, useState } from "react";
import {
  Activity,
  AlertCircle,
  CheckCircle2,
  Download,
  FilePlus2,
  LoaderCircle,
  PlayCircle,
  ScrollText,
  ShieldCheck,
  UserRound,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { getAuditLogs } from "../../api/auditLogsApi";
import type { AuditLog, AuditLogLevel } from "../../types/auditLog";
import "./AuditLogsPanel.css";

interface AuditLogsPanelProps {
  auditId: string;
}

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

function getEventIcon(eventType: string): LucideIcon {
  switch (eventType) {
    case "AuditCreated":
      return FilePlus2;
    case "AuditStarted":
      return PlayCircle;
    case "AuditCompleted":
      return CheckCircle2;
    case "AuditRulesEvaluated":
      return ShieldCheck;
    case "AuditExported":
      return Download;
    default:
      return Activity;
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

function AuditLogsPanel({ auditId }: AuditLogsPanelProps) {
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([]);

  const [loading, setLoading] = useState(true);

  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function loadAuditLogs() {
      try {
        setLoading(true);
        setError("");

        const result = await getAuditLogs(auditId);

        if (!cancelled) {
          setAuditLogs(result);
        }
      } catch {
        if (!cancelled) {
          setError("Impossible de récupérer le journal de l’audit.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadAuditLogs();

    return () => {
      cancelled = true;
    };
  }, [auditId]);

  return (
    <section className="audit-logs-panel">
      <div className="audit-logs-header">
        <div>
          <h2>Journal de l’audit</h2>

          <p>Historique des opérations importantes réalisées sur cet audit.</p>
        </div>

        {!loading && !error && (
          <span className="audit-logs-count">
            {auditLogs.length} événement(s)
          </span>
        )}
      </div>

      {loading && (
        <div className="audit-logs-state">
          <LoaderCircle className="audit-logs-spin" size={24} />
          Chargement du journal...
        </div>
      )}

      {error && (
        <div className="audit-logs-state error">
          <AlertCircle size={22} />
          {error}
        </div>
      )}

      {!loading && !error && auditLogs.length === 0 && (
        <div className="audit-logs-empty">
          <ScrollText size={34} />

          <strong>Aucun événement enregistré</strong>

          <span>Les prochaines opérations apparaîtront dans ce journal.</span>
        </div>
      )}

      {!loading && !error && auditLogs.length > 0 && (
        <div className="audit-logs-list">
          {auditLogs.map((auditLog) => {
            const EventIcon = getEventIcon(auditLog.eventType);

            return (
              <article
                className={`audit-log-entry ${auditLog.level.toLowerCase()}`}
                key={auditLog.id}
              >
                <div className="audit-log-icon">
                  <EventIcon size={20} />
                </div>

                <div className="audit-log-content">
                  <div className="audit-log-title">
                    <div>
                      <strong>{getEventLabel(auditLog.eventType)}</strong>

                      <span
                        className={`audit-log-level ${auditLog.level.toLowerCase()}`}
                      >
                        {getLevelLabel(auditLog.level)}
                      </span>
                    </div>

                    <time>{formatDate(auditLog.createdAt)}</time>
                  </div>

                  <p>{auditLog.message}</p>

                  <div className="audit-log-author">
                    <UserRound size={15} />

                    <span>
                      {auditLog.applicationUserDisplayName ?? "Système"}

                      {auditLog.applicationUserEmail &&
                        ` · ${auditLog.applicationUserEmail}`}
                    </span>
                  </div>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </section>
  );
}

export default AuditLogsPanel;
