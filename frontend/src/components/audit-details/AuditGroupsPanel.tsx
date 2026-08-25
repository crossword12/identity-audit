import { useEffect, useMemo, useState } from "react";
import {
  GitBranch,
  Link2,
  LoaderCircle,
  RefreshCw,
  Search,
  ShieldCheck,
  Users,
} from "lucide-react";
import {
  getAuditGroupMemberships,
  getAuditGroups,
} from "../../api/directoryAccessApi";
import type {
  DirectoryGroup,
  GroupMembership,
} from "../../types/directoryAccess";
import "./AuditGroupsPanel.css";

type GroupFilter = "All" | "Privileged" | "WithMembers" | "WithoutMembers";

interface AuditGroupsPanelProps {
  auditId: string;
}

function formatDate(date: string): string {
  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(date));
}

function AuditGroupsPanel({ auditId }: AuditGroupsPanelProps) {
  const [groups, setGroups] = useState<DirectoryGroup[]>([]);
  const [memberships, setMemberships] = useState<GroupMembership[]>([]);
  const [filter, setFilter] = useState<GroupFilter>("All");
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    let cancelled = false;

    async function loadGroups() {
      try {
        setLoading(true);
        setError("");

        const [groupResult, membershipResult] = await Promise.all([
          getAuditGroups(auditId),
          getAuditGroupMemberships(auditId),
        ]);

        if (!cancelled) {
          setGroups(groupResult);
          setMemberships(membershipResult);
        }
      } catch {
        if (!cancelled) {
          setError("Impossible de récupérer les groupes et leurs membres.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadGroups();

    return () => {
      cancelled = true;
    };
  }, [auditId, refreshKey]);

  const membershipsByGroup = useMemo(() => {
    const result = new Map<string, GroupMembership[]>();

    memberships.forEach((membership) => {
      const current = result.get(membership.groupId) ?? [];

      current.push(membership);
      result.set(membership.groupId, current);
    });

    return result;
  }, [memberships]);

  const filteredGroups = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return groups.filter((group) => {
      const groupMemberships = membershipsByGroup.get(group.id) ?? [];

      const matchesSearch =
        !normalizedSearch ||
        group.name.toLowerCase().includes(normalizedSearch) ||
        group.description?.toLowerCase().includes(normalizedSearch) ||
        group.externalId.toLowerCase().includes(normalizedSearch);

      if (!matchesSearch) {
        return false;
      }

      switch (filter) {
        case "Privileged":
          return group.isPrivileged;
        case "WithMembers":
          return groupMemberships.length > 0;
        case "WithoutMembers":
          return groupMemberships.length === 0;
        case "All":
          return true;
      }
    });
  }, [groups, membershipsByGroup, filter, search]);

  const privilegedGroupCount = groups.filter(
    (group) => group.isPrivileged,
  ).length;

  const directMembershipCount = memberships.filter(
    (membership) => membership.membershipType === "Direct",
  ).length;

  const transitiveMembershipCount = memberships.filter(
    (membership) => membership.membershipType === "Transitive",
  ).length;

  return (
    <section className="groups-panel">
      <div className="group-summary-grid">
        <article>
          <div className="group-summary-icon total">
            <Users size={21} />
          </div>

          <div>
            <span>Groupes collectés</span>
            <strong>{groups.length}</strong>
          </div>
        </article>

        <article>
          <div className="group-summary-icon privileged">
            <ShieldCheck size={21} />
          </div>

          <div>
            <span>Groupes privilégiés</span>
            <strong>{privilegedGroupCount}</strong>
          </div>
        </article>

        <article>
          <div className="group-summary-icon direct">
            <Link2 size={21} />
          </div>

          <div>
            <span>Appartenances directes</span>
            <strong>{directMembershipCount}</strong>
          </div>
        </article>

        <article>
          <div className="group-summary-icon transitive">
            <GitBranch size={21} />
          </div>

          <div>
            <span>Appartenances transitives</span>
            <strong>{transitiveMembershipCount}</strong>
          </div>
        </article>
      </div>

      <div className="groups-table-panel">
        <div className="groups-toolbar">
          <div>
            <h2>Groupes d’annuaire</h2>
            <span>{filteredGroups.length} résultat(s)</span>
          </div>

          <div className="group-toolbar-actions">
            <label className="group-search">
              <Search size={17} />

              <input
                type="search"
                value={search}
                placeholder="Rechercher un groupe..."
                onChange={(event) => setSearch(event.target.value)}
              />
            </label>

            <select
              value={filter}
              aria-label="Filtrer les groupes"
              onChange={(event) => setFilter(event.target.value as GroupFilter)}
            >
              <option value="All">Tous les groupes</option>
              <option value="Privileged">Groupes privilégiés</option>
              <option value="WithMembers">Avec membres collectés</option>
              <option value="WithoutMembers">Sans membre collecté</option>
            </select>

            <button
              className="refresh-groups-button"
              type="button"
              disabled={loading}
              onClick={() => setRefreshKey((value) => value + 1)}
            >
              {loading ? (
                <LoaderCircle className="group-spin" size={17} />
              ) : (
                <RefreshCw size={17} />
              )}
              Actualiser
            </button>
          </div>
        </div>

        {loading && (
          <div className="groups-message">Chargement des groupes...</div>
        )}

        {!loading && error && (
          <div className="groups-message error">{error}</div>
        )}

        {!loading && !error && filteredGroups.length === 0 && (
          <div className="groups-message">
            Aucun groupe ne correspond aux critères.
          </div>
        )}

        {!loading && !error && filteredGroups.length > 0 && (
          <div className="groups-table-container">
            <table className="groups-table">
              <thead>
                <tr>
                  <th>Groupe</th>
                  <th>Type</th>
                  <th>Niveau</th>
                  <th>Membres collectés</th>
                  <th>Collecté le</th>
                </tr>
              </thead>

              <tbody>
                {filteredGroups.map((group) => {
                  const groupMemberships =
                    membershipsByGroup.get(group.id) ?? [];

                  return (
                    <tr key={group.id}>
                      <td>
                        <strong>{group.name}</strong>

                        {group.description && <span>{group.description}</span>}

                        <small>ID externe : {group.externalId}</small>
                      </td>

                      <td>
                        <span className="group-type-badge">
                          {group.groupType ?? "Non renseigné"}
                        </span>
                      </td>

                      <td>
                        <span
                          className={`group-privilege-badge ${
                            group.isPrivileged ? "privileged" : "standard"
                          }`}
                        >
                          {group.isPrivileged ? "Privilégié" : "Standard"}
                        </span>
                      </td>

                      <td>
                        {groupMemberships.length === 0 ? (
                          <span className="group-no-members">Aucun</span>
                        ) : (
                          <details className="group-members-details">
                            <summary>
                              {groupMemberships.length} membre(s)
                            </summary>

                            <div className="group-member-list">
                              {groupMemberships.map((membership) => (
                                <div
                                  className="group-member"
                                  key={membership.id}
                                >
                                  <span>{membership.identityDisplayName}</span>

                                  <small
                                    className={`membership-type ${membership.membershipType.toLowerCase()}`}
                                  >
                                    {membership.membershipType}
                                  </small>
                                </div>
                              ))}
                            </div>
                          </details>
                        )}
                      </td>

                      <td>{formatDate(group.collectedAt)}</td>
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

export default AuditGroupsPanel;
