import axios from "axios";
import { useEffect, useState } from "react";
import {
  CheckCircle2,
  Cloud,
  LoaderCircle,
  Network,
  PlugZap,
  Server,
  XCircle,
  Pencil,
  Plus,
} from "lucide-react";
import { getTargets, testTargetConnection } from "../../api/targetsApi";
import type { Target } from "../../types/target";
import "./TargetsPage.css";
import TargetFormModal from "../../components/targets/TargetFormModal";

interface ConnectionFeedback {
  succeeded: boolean;
  message: string;
}

function formatDate(date: string | null): string {
  if (!date) {
    return "Jamais collectée";
  }

  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(date));
}

function getConfigurationSummary(target: Target): string {
  if (!target.configurationJson) {
    return "Configuration non renseignée";
  }

  try {
    const configuration = JSON.parse(target.configurationJson) as {
      domain?: string;
      environment?: string;
    };

    return (
      configuration.domain ??
      configuration.environment ??
      "Configuration disponible"
    );
  } catch {
    return "Configuration disponible";
  }
}

function TargetsPage() {
  const [targets, setTargets] = useState<Target[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [testingId, setTestingId] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<Record<string, ConnectionFeedback>>(
    {},
  );
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingTarget, setEditingTarget] = useState<Target | null>(null);

  useEffect(() => {
    async function loadTargets() {
      try {
        setLoading(true);
        setError("");
        const result = await getTargets();
        setTargets(result);
      } catch {
        setError("Impossible de récupérer les cibles.");
      } finally {
        setLoading(false);
      }
    }

    void loadTargets();
  }, []);

  async function handleTestConnection(targetId: string) {
    try {
      setTestingId(targetId);

      const result = await testTargetConnection(targetId);

      setFeedback((current) => ({
        ...current,
        [targetId]: {
          succeeded: result.succeeded,
          message: result.message,
        },
      }));
    } catch (requestError) {
      let message = "Le test de connexion a échoué.";

      if (axios.isAxiosError(requestError)) {
        const response = requestError.response?.data as
          | { message?: string }
          | undefined;

        message = response?.message ?? message;
      }

      setFeedback((current) => ({
        ...current,
        [targetId]: {
          succeeded: false,
          message,
        },
      }));
    } finally {
      setTestingId(null);
    }
  }

  function handleTargetSaved(savedTarget: Target) {
    setTargets((currentTargets) => {
      const targetExists = currentTargets.some(
        (target) => target.id === savedTarget.id,
      );

      if (targetExists) {
        return currentTargets.map((target) =>
          target.id === savedTarget.id ? savedTarget : target,
        );
      }

      return [...currentTargets, savedTarget];
    });

    setEditingTarget(null);
    setIsFormOpen(false);
  }

  return (
    <div className="targets-page">
      <div className="targets-header">
        <div>
          <h1>Cibles</h1>
          <p>
            Gestion des environnements Microsoft Entra ID et Active Directory.
          </p>
        </div>

        <div className="targets-header-actions">
          <div className="target-count">
            <Server size={18} />
            {targets.length} cible(s)
          </div>

          <button
            className="new-target-button"
            type="button"
            onClick={() => {
              setEditingTarget(null);
              setIsFormOpen(true);
            }}
          >
            <Plus size={18} />
            Nouvelle cible
          </button>
        </div>
      </div>

      {loading && (
        <div className="targets-message">Chargement des cibles...</div>
      )}

      {!loading && error && (
        <div className="targets-message targets-error">{error}</div>
      )}

      {!loading && !error && targets.length === 0 && (
        <div className="targets-message">
          Aucune cible n’est actuellement configurée.
        </div>
      )}

      {!loading && !error && targets.length > 0 && (
        <div className="targets-grid">
          {targets.map((target) => {
            const targetFeedback = feedback[target.id];
            const isTesting = testingId === target.id;

            return (
              <article className="target-card" key={target.id}>
                <div className="target-card-header">
                  <div
                    className={`target-type-icon ${
                      target.type === "EntraId" ? "entra" : "ad"
                    }`}
                  >
                    {target.type === "EntraId" ? (
                      <Cloud size={24} />
                    ) : (
                      <Network size={24} />
                    )}
                  </div>

                  <span
                    className={`target-status ${
                      target.isEnabled ? "enabled" : "disabled"
                    }`}
                  >
                    {target.isEnabled ? "Active" : "Inactive"}
                  </span>
                </div>

                <div className="target-information">
                  <h2>{target.name}</h2>

                  <span className="target-type">
                    {target.type === "EntraId"
                      ? "Microsoft Entra ID"
                      : "Active Directory"}
                  </span>
                </div>

                <dl className="target-details">
                  <div>
                    <dt>Configuration</dt>
                    <dd>{getConfigurationSummary(target)}</dd>
                  </div>

                  <div>
                    <dt>Dernière collecte</dt>
                    <dd>{formatDate(target.lastCollectedAt)}</dd>
                  </div>

                  <div>
                    <dt>Dernière modification</dt>
                    <dd>{formatDate(target.updatedAt)}</dd>
                  </div>
                </dl>

                {targetFeedback && (
                  <div
                    className={`connection-feedback ${
                      targetFeedback.succeeded ? "success" : "failure"
                    }`}
                  >
                    {targetFeedback.succeeded ? (
                      <CheckCircle2 size={17} />
                    ) : (
                      <XCircle size={17} />
                    )}
                    <span>{targetFeedback.message}</span>
                  </div>
                )}

                <div className="target-card-actions">
                  <button
                    className="edit-target-button"
                    type="button"
                    onClick={() => {
                      setEditingTarget(target);
                      setIsFormOpen(true);
                    }}
                  >
                    <Pencil size={18} />
                    Modifier
                  </button>

                  <button
                    className="test-connection-button"
                    type="button"
                    disabled={isTesting}
                    onClick={() => void handleTestConnection(target.id)}
                  >
                    {isTesting ? (
                      <LoaderCircle className="spinning" size={18} />
                    ) : (
                      <PlugZap size={18} />
                    )}

                    {isTesting ? "Test en cours..." : "Tester la connexion"}
                  </button>
                </div>
              </article>
            );
          })}
        </div>
      )}
      {isFormOpen && (
        <TargetFormModal
          target={editingTarget}
          onClose={() => {
            setEditingTarget(null);
            setIsFormOpen(false);
          }}
          onSaved={handleTargetSaved}
        />
      )}
    </div>
  );
}

export default TargetsPage;
