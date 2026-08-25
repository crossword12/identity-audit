import { useEffect, useMemo, useState } from "react";
import {
  BadgeCheck,
  KeyRound,
  LoaderCircle,
  RefreshCw,
  Search,
  ShieldCheck,
  Users,
} from "lucide-react";
import {
  getAuditRoleAssignments,
  getAuditRoles,
} from "../../api/directoryAccessApi";
import type {
  DirectoryRole,
  RoleAssignment,
} from "../../types/directoryAccess";
import "./AuditRolesPanel.css";

type RoleFilter =
  | "All"
  | "Privileged"
  | "WithAssignments"
  | "WithoutAssignments"
  | "WithExpiredAssignments";

interface AuditRolesPanelProps {
  auditId: string;
}

function formatDate(date: string | null): string {
  if (!date) {
    return "—";
  }

  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(date));
}

function isAssignmentExpired(assignment: RoleAssignment): boolean {
  return (
    !assignment.isPermanent &&
    assignment.expiresAt !== null &&
    new Date(assignment.expiresAt).getTime() < Date.now()
  );
}

function AuditRolesPanel({ auditId }: AuditRolesPanelProps) {
  const [roles, setRoles] = useState<DirectoryRole[]>([]);
  const [assignments, setAssignments] = useState<RoleAssignment[]>([]);
  const [filter, setFilter] = useState<RoleFilter>("All");
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    let cancelled = false;

    async function loadRoles() {
      try {
        setLoading(true);
        setError("");

        const [roleResult, assignmentResult] = await Promise.all([
          getAuditRoles(auditId),
          getAuditRoleAssignments(auditId),
        ]);

        if (!cancelled) {
          setRoles(roleResult);
          setAssignments(assignmentResult);
        }
      } catch {
        if (!cancelled) {
          setError("Impossible de récupérer les rôles et leurs affectations.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadRoles();

    return () => {
      cancelled = true;
    };
  }, [auditId, refreshKey]);

  const assignmentsByRole = useMemo(() => {
    const result = new Map<string, RoleAssignment[]>();

    assignments.forEach((assignment) => {
      const current = result.get(assignment.directoryRoleId) ?? [];

      current.push(assignment);
      result.set(assignment.directoryRoleId, current);
    });

    return result;
  }, [assignments]);

  const filteredRoles = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return roles.filter((role) => {
      const roleAssignments = assignmentsByRole.get(role.id) ?? [];

      const matchesSearch =
        !normalizedSearch ||
        role.name.toLowerCase().includes(normalizedSearch) ||
        role.description?.toLowerCase().includes(normalizedSearch) ||
        role.externalId.toLowerCase().includes(normalizedSearch) ||
        roleAssignments.some((assignment) =>
          assignment.identityDisplayName
            .toLowerCase()
            .includes(normalizedSearch),
        );

      if (!matchesSearch) {
        return false;
      }

      switch (filter) {
        case "Privileged":
          return role.isPrivileged;
        case "WithAssignments":
          return roleAssignments.length > 0;
        case "WithoutAssignments":
          return roleAssignments.length === 0;
        case "WithExpiredAssignments":
          return roleAssignments.some(isAssignmentExpired);
        case "All":
          return true;
      }
    });
  }, [roles, assignmentsByRole, filter, search]);

  const privilegedRoleCount = roles.filter((role) => role.isPrivileged).length;

  const permanentAssignmentCount = assignments.filter(
    (assignment) => assignment.isPermanent,
  ).length;

  return (
    <section className="roles-panel">
      <div className="role-summary-grid">
        <article>
          <div className="role-summary-icon total">
            <KeyRound size={21} />
          </div>

          <div>
            <span>Rôles collectés</span>
            <strong>{roles.length}</strong>
          </div>
        </article>

        <article>
          <div className="role-summary-icon privileged">
            <ShieldCheck size={21} />
          </div>

          <div>
            <span>Rôles privilégiés</span>
            <strong>{privilegedRoleCount}</strong>
          </div>
        </article>

        <article>
          <div className="role-summary-icon assignments">
            <Users size={21} />
          </div>

          <div>
            <span>Affectations collectées</span>
            <strong>{assignments.length}</strong>
          </div>
        </article>

        <article>
          <div className="role-summary-icon permanent">
            <BadgeCheck size={21} />
          </div>

          <div>
            <span>Affectations permanentes</span>
            <strong>{permanentAssignmentCount}</strong>
          </div>
        </article>
      </div>

      <div className="roles-table-panel">
        <div className="roles-toolbar">
          <div>
            <h2>Rôles d’annuaire</h2>
            <span>{filteredRoles.length} résultat(s)</span>
          </div>

          <div className="role-toolbar-actions">
            <label className="role-search">
              <Search size={17} />

              <input
                type="search"
                value={search}
                placeholder="Rechercher un rôle..."
                onChange={(event) => setSearch(event.target.value)}
              />
            </label>

            <select
              value={filter}
              aria-label="Filtrer les rôles"
              onChange={(event) => setFilter(event.target.value as RoleFilter)}
            >
              <option value="All">Tous les rôles</option>
              <option value="Privileged">Rôles privilégiés</option>
              <option value="WithAssignments">Avec affectations</option>
              <option value="WithoutAssignments">Sans affectation</option>
              <option value="WithExpiredAssignments">
                Avec affectation expirée
              </option>
            </select>

            <button
              className="refresh-roles-button"
              type="button"
              disabled={loading}
              onClick={() => setRefreshKey((value) => value + 1)}
            >
              {loading ? (
                <LoaderCircle className="role-spin" size={17} />
              ) : (
                <RefreshCw size={17} />
              )}
              Actualiser
            </button>
          </div>
        </div>

        {loading && (
          <div className="roles-message">Chargement des rôles...</div>
        )}

        {!loading && error && (
          <div className="roles-message error">{error}</div>
        )}

        {!loading && !error && filteredRoles.length === 0 && (
          <div className="roles-message">
            Aucun rôle ne correspond aux critères.
          </div>
        )}

        {!loading && !error && filteredRoles.length > 0 && (
          <div className="roles-table-container">
            <table className="roles-table">
              <thead>
                <tr>
                  <th>Rôle</th>
                  <th>Source</th>
                  <th>Niveau</th>
                  <th>Affectations</th>
                  <th>Collecté le</th>
                </tr>
              </thead>

              <tbody>
                {filteredRoles.map((role) => {
                  const roleAssignments = assignmentsByRole.get(role.id) ?? [];

                  return (
                    <tr key={role.id}>
                      <td>
                        <strong>{role.name}</strong>

                        {role.description && <span>{role.description}</span>}

                        <small>ID externe : {role.externalId}</small>
                      </td>

                      <td>
                        <span className="role-source-badge">
                          {role.source === "EntraId"
                            ? "Microsoft Entra ID"
                            : role.source}
                        </span>
                      </td>

                      <td>
                        <span
                          className={`role-privilege-badge ${
                            role.isPrivileged ? "privileged" : "standard"
                          }`}
                        >
                          {role.isPrivileged ? "Privilégié" : "Standard"}
                        </span>
                      </td>

                      <td>
                        {roleAssignments.length === 0 ? (
                          <span className="role-no-assignments">Aucune</span>
                        ) : (
                          <details className="role-assignments-details">
                            <summary>
                              {roleAssignments.length} affectation(s)
                            </summary>

                            <div className="role-assignment-list">
                              {roleAssignments.map((assignment) => {
                                const expired = isAssignmentExpired(assignment);

                                return (
                                  <article
                                    className="role-assignment"
                                    key={assignment.id}
                                  >
                                    <div className="role-assignment-header">
                                      <strong>
                                        {assignment.identityDisplayName}
                                      </strong>

                                      <span
                                        className={`assignment-status ${
                                          expired
                                            ? "expired"
                                            : assignment.isPermanent
                                              ? "permanent"
                                              : "temporary"
                                        }`}
                                      >
                                        {expired
                                          ? "Expirée"
                                          : assignment.isPermanent
                                            ? "Permanente"
                                            : "Temporaire"}
                                      </span>
                                    </div>

                                    <dl>
                                      <div>
                                        <dt>Affectée le</dt>
                                        <dd>
                                          {formatDate(assignment.assignedAt)}
                                        </dd>
                                      </div>

                                      <div>
                                        <dt>Expiration</dt>
                                        <dd>
                                          {assignment.isPermanent
                                            ? "Aucune"
                                            : formatDate(assignment.expiresAt)}
                                        </dd>
                                      </div>
                                    </dl>
                                  </article>
                                );
                              })}
                            </div>
                          </details>
                        )}
                      </td>

                      <td>{formatDate(role.collectedAt)}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  );
}

export default AuditRolesPanel;
