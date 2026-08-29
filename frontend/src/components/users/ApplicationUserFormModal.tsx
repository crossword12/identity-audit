import axios from "axios";
import { useState, type FormEvent } from "react";
import { Eye, EyeOff, Pencil, UserPlus, X } from "lucide-react";
import {
  createApplicationUser,
  updateApplicationUser,
} from "../../api/applicationUsersApi";
import { ApplicationRoles } from "../../auth/authorization";
import type { ApplicationRole } from "../../types/auth";
import type { ApplicationUser } from "../../types/applicationUser";
import "./ApplicationUserModal.css";

interface ApplicationUserFormModalProps {
  user: ApplicationUser | null;
  onClose: () => void;
  onSaved: (user: ApplicationUser) => void;
}

const roleOptions: Array<{
  value: ApplicationRole;
  label: string;
}> = [
  {
    value: ApplicationRoles.Administrator,
    label: "Administrateur",
  },
  {
    value: ApplicationRoles.Auditor,
    label: "Auditeur",
  },
  {
    value: ApplicationRoles.Reader,
    label: "Lecteur",
  },
];

function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const response = error.response?.data as
      | { message?: string; title?: string }
      | undefined;

    return response?.message ?? response?.title ?? "L’opération a échoué.";
  }

  return "L’opération a échoué.";
}

function ApplicationUserFormModal({
  user,
  onClose,
  onSaved,
}: ApplicationUserFormModalProps) {
  const isEditing = user !== null;

  const [email, setEmail] = useState(user?.email ?? "");

  const [displayName, setDisplayName] = useState(user?.displayName ?? "");

  const [password, setPassword] = useState("");

  const [isEnabled, setIsEnabled] = useState(user?.isEnabled ?? true);

  const [roles, setRoles] = useState<ApplicationRole[]>(
    user ? [] : [ApplicationRoles.Reader],
  );

  const [showPassword, setShowPassword] = useState(false);

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

    const normalizedEmail = email.trim();
    const normalizedDisplayName = displayName.trim();

    if (!normalizedEmail || normalizedDisplayName.length < 2) {
      setError("Renseignez une adresse électronique et un nom valide.");

      return;
    }

    if (!isEditing && (password.length < 12 || roles.length === 0)) {
      setError(
        "Le mot de passe doit contenir au moins 12 caractères et un rôle doit être sélectionné.",
      );

      return;
    }

    try {
      setSubmitting(true);
      setError("");

      const savedUser = isEditing
        ? await updateApplicationUser(user.id, {
            email: normalizedEmail,
            displayName: normalizedDisplayName,
            isEnabled,
          })
        : await createApplicationUser({
            email: normalizedEmail,
            displayName: normalizedDisplayName,
            password,
            roles,
          });

      onSaved(savedUser);
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
        aria-labelledby="application-user-modal-title"
      >
        <header className="application-user-modal-header">
          <div className="application-user-modal-icon">
            {isEditing ? <Pencil size={21} /> : <UserPlus size={21} />}
          </div>

          <div>
            <h2 id="application-user-modal-title">
              {isEditing ? "Modifier l’utilisateur" : "Nouvel utilisateur"}
            </h2>

            <p>
              {isEditing
                ? "Modifiez les informations et l’état du compte."
                : "Créez un nouveau compte applicatif."}
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
          <div className="application-user-form-grid">
            <label className="application-user-field">
              <span>Nom affiché</span>

              <input
                type="text"
                value={displayName}
                minLength={2}
                maxLength={255}
                required
                autoFocus
                onChange={(event) => setDisplayName(event.target.value)}
              />
            </label>

            <label className="application-user-field">
              <span>Adresse électronique</span>

              <input
                type="email"
                value={email}
                maxLength={320}
                required
                onChange={(event) => setEmail(event.target.value)}
              />
            </label>
          </div>

          {!isEditing && (
            <>
              <label className="application-user-field">
                <span>Mot de passe initial</span>

                <div className="application-user-password">
                  <input
                    type={showPassword ? "text" : "password"}
                    value={password}
                    minLength={12}
                    maxLength={200}
                    required
                    autoComplete="new-password"
                    onChange={(event) => setPassword(event.target.value)}
                  />

                  <button
                    type="button"
                    onClick={() => setShowPassword((value) => !value)}
                    aria-label={
                      showPassword
                        ? "Masquer le mot de passe"
                        : "Afficher le mot de passe"
                    }
                  >
                    {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>

                <small>
                  Au moins 12 caractères, avec majuscule, minuscule, chiffre et
                  caractère spécial.
                </small>
              </label>

              <fieldset className="application-user-roles">
                <legend>Rôles</legend>

                <div className="application-user-role-options">
                  {roleOptions.map((role) => (
                    <label key={role.value}>
                      <input
                        type="checkbox"
                        checked={roles.includes(role.value)}
                        onChange={() => toggleRole(role.value)}
                      />

                      <span>{role.label}</span>
                    </label>
                  ))}
                </div>
              </fieldset>
            </>
          )}

          {isEditing && (
            <label className="application-user-enabled">
              <input
                type="checkbox"
                checked={isEnabled}
                onChange={(event) => setIsEnabled(event.target.checked)}
              />

              <span>Compte autorisé à se connecter</span>
            </label>
          )}

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
              {submitting
                ? "Enregistrement..."
                : isEditing
                  ? "Enregistrer"
                  : "Créer l’utilisateur"}
            </button>
          </footer>
        </form>
      </section>
    </div>
  );
}

export default ApplicationUserFormModal;
