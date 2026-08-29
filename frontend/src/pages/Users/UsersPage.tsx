import { useEffect, useMemo, useState } from "react";
import {
  RefreshCw,
  ShieldCheck,
  UserCheck,
  Users,
  UserX,
  Pencil,
  Plus,
} from "lucide-react";
import { getApplicationUsers } from "../../api/applicationUsersApi";
import type { ApplicationRole } from "../../types/auth";
import type { ApplicationUser } from "../../types/applicationUser";
import ApplicationUserFormModal from "../../components/users/ApplicationUserFormModal";
import ApplicationUserRolesModal from "../../components/users/ApplicationUserRolesModal";
import "./UsersPage.css";

const roleLabels: Record<ApplicationRole, string> = {
  Administrator: "Administrateur",
  Auditor: "Auditeur",
  Reader: "Lecteur",
};

function formatDate(date: string): string {
  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(date));
}

function getInitials(displayName: string): string {
  return displayName
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();
}

function UsersPage() {
  const [users, setUsers] = useState<ApplicationUser[]>([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [refreshKey, setRefreshKey] = useState(0);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<ApplicationUser | null>(null);
  const [rolesUser, setRolesUser] = useState<ApplicationUser | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadUsers() {
      try {
        setLoading(true);
        setError("");

        const result = await getApplicationUsers();

        if (!cancelled) {
          setUsers(result);
        }
      } catch {
        if (!cancelled) {
          setError("Impossible de récupérer les utilisateurs.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadUsers();

    return () => {
      cancelled = true;
    };
  }, [refreshKey]);

  const enabledCount = useMemo(
    () => users.filter((user) => user.isEnabled).length,
    [users],
  );

  const disabledCount = users.length - enabledCount;

  const administratorCount = useMemo(
    () => users.filter((user) => user.roles.includes("Administrator")).length,
    [users],
  );
  function handleUserSaved(savedUser: ApplicationUser) {
    setUsers((currentUsers) => {
      const userExists = currentUsers.some((user) => user.id === savedUser.id);

      if (userExists) {
        return currentUsers.map((user) =>
          user.id === savedUser.id ? savedUser : user,
        );
      }

      return [...currentUsers, savedUser];
    });

    setEditingUser(null);
    setRolesUser(null);
    setIsFormOpen(false);
  }
  return (
    <div className="users-page">
      <div className="users-header">
        <div>
          <h1>Utilisateurs</h1>

          <p>Gestion des comptes applicatifs et de leurs autorisations.</p>
        </div>

        <div className="users-header-actions">
          <div className="users-count">
            <Users size={18} />
            {users.length} utilisateur(s)
          </div>

          <button
            className="refresh-users-button"
            type="button"
            disabled={loading}
            onClick={() => setRefreshKey((value) => value + 1)}
          >
            <RefreshCw className={loading ? "users-spin" : ""} size={18} />
            Actualiser
          </button>
          <button
            className="new-user-button"
            type="button"
            onClick={() => {
              setEditingUser(null);
              setIsFormOpen(true);
            }}
          >
            <Plus size={18} />
            Nouvel utilisateur
          </button>
        </div>
      </div>

      <div className="users-summary-grid">
        <article className="users-summary-card">
          <div className="users-summary-icon enabled">
            <UserCheck size={22} />
          </div>

          <div>
            <span>Comptes actifs</span>
            <strong>{enabledCount}</strong>
          </div>
        </article>

        <article className="users-summary-card">
          <div className="users-summary-icon disabled">
            <UserX size={22} />
          </div>

          <div>
            <span>Comptes désactivés</span>
            <strong>{disabledCount}</strong>
          </div>
        </article>

        <article className="users-summary-card">
          <div className="users-summary-icon administrators">
            <ShieldCheck size={22} />
          </div>

          <div>
            <span>Administrateurs</span>
            <strong>{administratorCount}</strong>
          </div>
        </article>
      </div>

      {loading && (
        <div className="users-message">Chargement des utilisateurs...</div>
      )}

      {!loading && error && (
        <div className="users-message users-error">{error}</div>
      )}

      {!loading && !error && (
        <section className="users-panel">
          <div className="users-panel-header">
            <div>
              <h2>Comptes applicatifs</h2>
              <span>{users.length} résultat(s)</span>
            </div>
          </div>

          {users.length === 0 ? (
            <div className="users-empty">Aucun utilisateur applicatif.</div>
          ) : (
            <div className="users-table-container">
              <table className="users-table">
                <thead>
                  <tr>
                    <th>Utilisateur</th>
                    <th>État</th>
                    <th>Rôles</th>
                    <th>Créé le</th>
                    <th>Modifié le</th>
                    <th>Actions</th>
                  </tr>
                </thead>

                <tbody>
                  {users.map((user) => (
                    <tr key={user.id}>
                      <td>
                        <div className="user-cell">
                          <span className="user-list-avatar">
                            {getInitials(user.displayName)}
                          </span>

                          <div>
                            <strong>{user.displayName}</strong>

                            <span>{user.email}</span>
                          </div>
                        </div>
                      </td>

                      <td>
                        <span
                          className={`user-state ${
                            user.isEnabled ? "enabled" : "disabled"
                          }`}
                        >
                          {user.isEnabled ? "Actif" : "Désactivé"}
                        </span>
                      </td>

                      <td>
                        <div className="user-role-list">
                          {user.roles.map((role) => (
                            <span
                              className={`user-role ${role.toLowerCase()}`}
                              key={role}
                            >
                              {roleLabels[role]}
                            </span>
                          ))}
                        </div>
                      </td>

                      <td>{formatDate(user.createdAt)}</td>

                      <td>{formatDate(user.updatedAt)}</td>
                      <td>
                        <div className="user-row-actions">
                          <button
                            className="edit-user-button"
                            type="button"
                            onClick={() => {
                              setEditingUser(user);
                              setIsFormOpen(true);
                            }}
                          >
                            <Pencil size={16} />
                            Modifier
                          </button>
                          <button
                            className="roles-user-button"
                            type="button"
                            onClick={() => setRolesUser(user)}
                          >
                            <ShieldCheck size={16} />
                            Rôles
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      )}
      {isFormOpen && (
        <ApplicationUserFormModal
          user={editingUser}
          onClose={() => {
            setEditingUser(null);
            setIsFormOpen(false);
          }}
          onSaved={handleUserSaved}
        />
      )}
      {rolesUser && (
        <ApplicationUserRolesModal
          user={rolesUser}
          onClose={() => setRolesUser(null)}
          onSaved={handleUserSaved}
        />
      )}
    </div>
  );
}

export default UsersPage;
