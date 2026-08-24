import axios from "axios";
import { useState } from "react";
import type { FormEvent } from "react";
import { X } from "lucide-react";
import { createTarget, updateTarget } from "../../api/targetsApi";
import type { Target, TargetType } from "../../types/target";
import "./TargetFormModal.css";

interface TargetFormModalProps {
  target: Target | null;
  onClose: () => void;
  onSaved: (target: Target) => void;
}

function TargetFormModal({ target, onClose, onSaved }: TargetFormModalProps) {
  const isEditing = target !== null;

  const [name, setName] = useState(target?.name ?? "");
  const [type, setType] = useState<TargetType>(
    target?.type ?? "ActiveDirectory",
  );
  const [isEnabled, setIsEnabled] = useState(target?.isEnabled ?? true);
  const [configurationJson, setConfigurationJson] = useState(
    target?.configurationJson ?? '{\n  "environment": "test"\n}',
  );
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    if (!name.trim()) {
      setError("Le nom de la cible est obligatoire.");
      return;
    }

    let normalizedConfiguration: string | null = null;

    if (configurationJson.trim()) {
      try {
        normalizedConfiguration = JSON.stringify(JSON.parse(configurationJson));
      } catch {
        setError("La configuration JSON n’est pas valide.");
        return;
      }
    }

    try {
      setSubmitting(true);

      const savedTarget = isEditing
        ? await updateTarget(target.id, {
            name: name.trim(),
            isEnabled,
            configurationJson: normalizedConfiguration,
          })
        : await createTarget({
            name: name.trim(),
            type,
            isEnabled,
            configurationJson: normalizedConfiguration,
          });

      onSaved(savedTarget);
    } catch (requestError) {
      let message = "Impossible d’enregistrer la cible.";

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
      className="modal-overlay"
      role="presentation"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          onClose();
        }
      }}
    >
      <section
        className="target-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="target-modal-title"
      >
        <div className="target-modal-header">
          <div>
            <h2 id="target-modal-title">
              {isEditing ? "Modifier la cible" : "Nouvelle cible"}
            </h2>
            <p>
              {isEditing
                ? "Mettez à jour les paramètres de la cible."
                : "Configurez un nouvel environnement à auditer."}
            </p>
          </div>

          <button
            className="close-modal-button"
            type="button"
            aria-label="Fermer"
            onClick={onClose}
          >
            <X size={21} />
          </button>
        </div>

        <form onSubmit={(event) => void handleSubmit(event)}>
          <div className="form-field">
            <label htmlFor="target-name">Nom de la cible</label>
            <input
              id="target-name"
              value={name}
              maxLength={255}
              placeholder="Ex. Active Directory Production"
              onChange={(event) => setName(event.target.value)}
            />
          </div>

          <div className="form-field">
            <label htmlFor="target-type">Type de cible</label>
            <select
              id="target-type"
              value={type}
              disabled={isEditing}
              onChange={(event) => setType(event.target.value as TargetType)}
            >
              <option value="ActiveDirectory">Active Directory</option>
              <option value="EntraId">Microsoft Entra ID</option>
            </select>

            {isEditing && (
              <small>
                Le type d’une cible existante ne peut pas être modifié.
              </small>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="target-configuration">Configuration JSON</label>
            <textarea
              id="target-configuration"
              rows={6}
              value={configurationJson}
              spellCheck={false}
              placeholder={'{\n  "environment": "test"\n}'}
              onChange={(event) => setConfigurationJson(event.target.value)}
            />
            <small>La configuration doit respecter le format JSON.</small>
          </div>

          <label className="enabled-field">
            <input
              type="checkbox"
              checked={isEnabled}
              onChange={(event) => setIsEnabled(event.target.checked)}
            />

            <span>
              <strong>Cible active</strong>
              <small>
                Autoriser l’utilisation de cette cible pour les audits.
              </small>
            </span>
          </label>

          {error && <div className="form-error">{error}</div>}

          <div className="target-modal-actions">
            <button
              className="secondary-button"
              type="button"
              disabled={submitting}
              onClick={onClose}
            >
              Annuler
            </button>

            <button
              className="primary-button"
              type="submit"
              disabled={submitting}
            >
              {submitting
                ? "Enregistrement..."
                : isEditing
                  ? "Enregistrer"
                  : "Créer la cible"}
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}

export default TargetFormModal;
