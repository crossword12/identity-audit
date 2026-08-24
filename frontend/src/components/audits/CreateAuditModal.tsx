import axios from "axios";
import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Info, X } from "lucide-react";
import { createAudit } from "../../api/auditsApi";
import { getTargets } from "../../api/targetsApi";
import type { Audit } from "../../types/audit";
import type { Target } from "../../types/target";
import "./CreateAuditModal.css";

interface CreateAuditModalProps {
  onClose: () => void;
  onCreated: (audit: Audit) => void;
}

function CreateAuditModal({ onClose, onCreated }: CreateAuditModalProps) {
  const [targets, setTargets] = useState<Target[]>([]);
  const [selectedTargetId, setSelectedTargetId] = useState("");
  const [loadingTargets, setLoadingTargets] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function loadTargets() {
      try {
        setLoadingTargets(true);
        setError("");

        const result = await getTargets();
        const enabledTargets = result.filter((target) => target.isEnabled);

        if (!cancelled) {
          setTargets(enabledTargets);
          setSelectedTargetId(enabledTargets[0]?.id ?? "");
        }
      } catch {
        if (!cancelled) {
          setError("Impossible de récupérer les cibles actives.");
        }
      } finally {
        if (!cancelled) {
          setLoadingTargets(false);
        }
      }
    }

    void loadTargets();

    return () => {
      cancelled = true;
    };
  }, []);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    if (!selectedTargetId) {
      setError("Veuillez sélectionner une cible active.");
      return;
    }

    try {
      setSubmitting(true);

      const createdAudit = await createAudit({
        targetId: selectedTargetId,
      });

      onCreated(createdAudit);
    } catch (requestError) {
      let message = "Impossible de créer l’audit.";

      if (axios.isAxiosError(requestError)) {
        const response = requestError.response?.data as
          | { message?: string; title?: string }
          | undefined;

        message = response?.message ?? response?.title ?? message;
      }

      setError(message);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div
      className="create-audit-overlay"
      role="presentation"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          onClose();
        }
      }}
    >
      <section
        className="create-audit-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="create-audit-title"
      >
        <div className="create-audit-header">
          <div>
            <h2 id="create-audit-title">Nouvel audit</h2>
            <p>Sélectionnez l’environnement à auditer.</p>
          </div>

          <button
            className="close-create-audit"
            type="button"
            aria-label="Fermer"
            onClick={onClose}
          >
            <X size={21} />
          </button>
        </div>

        <form onSubmit={(event) => void handleSubmit(event)}>
          <div className="create-audit-field">
            <label htmlFor="audit-target">Cible de l’audit</label>

            <select
              id="audit-target"
              value={selectedTargetId}
              disabled={loadingTargets || targets.length === 0}
              onChange={(event) => setSelectedTargetId(event.target.value)}
            >
              {loadingTargets && (
                <option value="">Chargement des cibles...</option>
              )}

              {!loadingTargets && targets.length === 0 && (
                <option value="">Aucune cible active</option>
              )}

              {targets.map((target) => (
                <option key={target.id} value={target.id}>
                  {target.name} —{" "}
                  {target.type === "EntraId"
                    ? "Microsoft Entra ID"
                    : "Active Directory"}
                </option>
              ))}
            </select>
          </div>

          <div className="audit-workflow-information">
            <Info size={19} />

            <div>
              <strong>Fonctionnement de l’audit</strong>
              <p>
                L’audit sera d’abord créé avec le statut « En attente ». Le
                démarrage ouvre ensuite la session d’audit, tandis que la
                collecte des identités reste exécutée séparément par le
                collecteur correspondant.
              </p>
            </div>
          </div>

          {error && <div className="create-audit-error">{error}</div>}

          <div className="create-audit-actions">
            <button
              className="cancel-create-audit"
              type="button"
              disabled={submitting}
              onClick={onClose}
            >
              Annuler
            </button>

            <button
              className="confirm-create-audit"
              type="submit"
              disabled={submitting || loadingTargets || targets.length === 0}
            >
              {submitting ? "Création..." : "Créer l’audit"}
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}

export default CreateAuditModal;
