import axios from "axios";
import { useState, type FormEvent } from "react";
import { ShieldCheck, X } from "lucide-react";
import { updateApplicationUserRoles } from "../../api/applicationUsersApi";
import { ApplicationRoles } from "../../auth/authorization";
import type { ApplicationRole } from "../../types/auth";
import type { ApplicationUser } from "../../types/applicationUser";
import "./ApplicationUserModal.css";

interface ApplicationUserRolesModalProps {
  user: ApplicationUser;
  onClose: () => void;
  onSaved: (user: ApplicationUser) => void;
}

const roleOptions: Array<{
  value: ApplicationRole;
  label: string;
  description: string;
}> = [
  {
    value: ApplicationRoles.Administrator,
    label: "Administrateur",
    description: "Gestion des cibles, utilisateurs et audits.",
  },
  {
    value: ApplicationRoles.Auditor,
    label: "Auditeur",
    description: "Création et exécution des audits.",
  },
  {
    value: ApplicationRoles.Reader,
    label: "Lecteur",
    description: "Consultation des données uniquement.",
  },
];

function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const response = error.response?.data as
      | { message?: string; title?: string }
      | undefined;

    return (
      response?.message ??
      response?.title ??
      "La modification des rôles a échoué."
    );
  }

  return "La modification des rôles a échoué.";
}

function ApplicationUserRolesModal({
  user,
  onClose,
  onSaved,
}: ApplicationUserRolesModalProps) {
  const [roles, setRoles] = useState<ApplicationRole[]>([...user.roles]);

  const [submitting, setSubmitting] = useState(false);

  const [error, setError] = useState("");

  function toggleRole(role: ApplicationRole) {
    setRoles((currentRoles) =>
      currentRoles.includes(role)
        ? currentRoles.filter((currentRole) => currentRole !== role)
        : [...currentRoles, role],
    );
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (roles.length === 0) {
      setError("Sélectionnez au moins un rôle.");

      return;
    }

    try {
      setSubmitting(true);
      setError("");

      const updatedUser = await updateApplicationUserRoles(user.id, { roles });

      onSaved(updatedUser);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="application-user-modal-backdrop" role="presentation">
      <section
        className="application-user-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="application-user-roles-title"
      >
        <header className="application-user-modal-header">
          <div className="application-user-modal-icon">
            <ShieldCheck size={21} />
          </div>

          <div>
            <h2 id="application-user-roles-title">Modifier les rôles</h2>

            <p>
              {user.displayName} · {user.email}
            </p>
          </div>

          <button
            className="application-user-modal-close"
            type="button"
            onClick={onClose}
            aria-label="Fermer"
          >
            <X size={20} />
          </button>
        </header>

        <form
          className="application-user-form"
          onSubmit={(event) => void handleSubmit(event)}
        >
          <fieldset className="application-user-roles">
            <legend>Rôles applicatifs</legend>

            <div className="application-user-role-options">
              {roleOptions.map((role) => (
                <label key={role.value}>
                  <input
                    type="checkbox"
                    checked={roles.includes(role.value)}
                    onChange={() => toggleRole(role.value)}
                  />

                  <span>
                    <strong>{role.label}</strong>
                    <small>{role.description}</small>
                  </span>
                </label>
              ))}
            </div>
          </fieldset>

          {error && <div className="application-user-modal-error">{error}</div>}

          <footer className="application-user-modal-actions">
            <button
              className="application-user-secondary-button"
              type="button"
              disabled={submitting}
              onClick={onClose}
            >
              Annuler
            </button>

            <button
              className="application-user-primary-button"
              type="submit"
              disabled={submitting}
            >
              {submitting ? "Enregistrement..." : "Enregistrer les rôles"}
            </button>
          </footer>
        </form>
      </section>
    </div>
  );
}

export default ApplicationUserRolesModal;
