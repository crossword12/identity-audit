import { useEffect, useMemo, useState } from "react";
import {
  LoaderCircle,
  RefreshCw,
  Search,
  ShieldCheck,
  UserX,
  Users,
  Wrench,
} from "lucide-react";
import { getAuditIdentities } from "../../api/identitiesApi";
import type { AccountType, DirectoryIdentity } from "../../types/identity";
import "./AuditIdentitiesPanel.css";

type IdentityFilter =
  | "All"
  | "Enabled"
  | "Disabled"
  | "Privileged"
  | "ServiceAccount"
  | "Locked";

interface AuditIdentitiesPanelProps {
  auditId: string;
}

function formatDate(date: string | null): string {
  if (!date) {
    return "Jamais";
  }

  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(date));
}

function getAccountTypeLabel(type: AccountType): string {
  switch (type) {
    case "User":
      return "Utilisateur";
    case "Guest":
      return "Invité";
    case "Administrator":
      return "Administrateur";
    case "ServiceAccount":
      return "Compte de service";
  }
}

function AuditIdentitiesPanel({ auditId }: AuditIdentitiesPanelProps) {
  const [identities, setIdentities] = useState<DirectoryIdentity[]>([]);
  const [filter, setFilter] = useState<IdentityFilter>("All");
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    let cancelled = false;

    async function loadIdentities() {
      try {
        setLoading(true);
        setError("");

        const result = await getAuditIdentities(auditId);

        if (!cancelled) {
          setIdentities(result);
        }
      } catch {
        if (!cancelled) {
          setError("Impossible de récupérer les identités collectées.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void loadIdentities();

    return () => {
      cancelled = true;
    };
  }, [auditId, refreshKey]);

  const filteredIdentities = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return identities.filter((identity) => {
      const matchesSearch =
        !normalizedSearch ||
        identity.displayName.toLowerCase().includes(normalizedSearch) ||
        identity.userName.toLowerCase().includes(normalizedSearch) ||
        identity.email?.toLowerCase().includes(normalizedSearch) ||
        identity.description?.toLowerCase().includes(normalizedSearch);

      if (!matchesSearch) {
        return false;
      }

      switch (filter) {
        case "Enabled":
          return identity.isEnabled;
        case "Disabled":
          return !identity.isEnabled;
        case "Privileged":
          return identity.isPrivileged;
        case "ServiceAccount":
          return identity.isServiceAccount;
        case "Locked":
          return identity.isLocked === true;
        case "All":
          return true;
      }
    });
  }, [identities, filter, search]);

  const privilegedCount = identities.filter(
    (identity) => identity.isPrivileged,
  ).length;

  const disabledCount = identities.filter(
    (identity) => !identity.isEnabled,
  ).length;

  const serviceAccountCount = identities.filter(
    (identity) => identity.isServiceAccount,
  ).length;

  return (
    <section className="identities-panel">
      <div className="identity-summary-grid">
        <article>
          <div className="identity-summary-icon total">
            <Users size={21} />
          </div>

          <div>
            <span>Identités collectées</span>
            <strong>{identities.length}</strong>
          </div>
        </article>

        <article>
          <div className="identity-summary-icon privileged">
            <ShieldCheck size={21} />
          </div>

          <div>
            <span>Comptes privilégiés</span>
            <strong>{privilegedCount}</strong>
          </div>
        </article>

        <article>
          <div className="identity-summary-icon disabled">
            <UserX size={21} />
          </div>

          <div>
            <span>Comptes désactivés</span>
            <strong>{disabledCount}</strong>
          </div>
        </article>

        <article>
          <div className="identity-summary-icon service">
            <Wrench size={21} />
          </div>

          <div>
            <span>Comptes de service</span>
            <strong>{serviceAccountCount}</strong>
          </div>
        </article>
      </div>

      <div className="identities-table-panel">
        <div className="identities-toolbar">
          <div>
            <h2>Identités collectées</h2>
            <span>{filteredIdentities.length} résultat(s)</span>
          </div>

          <div className="identity-toolbar-actions">
            <label className="identity-search">
              <Search size={17} />

              <input
                type="search"
                value={search}
                placeholder="Rechercher une identité..."
                onChange={(event) => setSearch(event.target.value)}
              />
            </label>

            <select
              value={filter}
              aria-label="Filtrer les identités"
              onChange={(event) =>
                setFilter(event.target.value as IdentityFilter)
              }
            >
              <option value="All">Toutes les identités</option>
              <option value="Enabled">Comptes actifs</option>
              <option value="Disabled">Comptes désactivés</option>
              <option value="Privileged">Comptes privilégiés</option>
              <option value="ServiceAccount">Comptes de service</option>
              <option value="Locked">Comptes verrouillés</option>
            </select>

            <button
              className="refresh-identities-button"
              type="button"
              disabled={loading}
              onClick={() => setRefreshKey((value) => value + 1)}
            >
              {loading ? (
                <LoaderCircle className="identity-spin" size={17} />
              ) : (
                <RefreshCw size={17} />
              )}
              Actualiser
            </button>
          </div>
        </div>

        {loading && (
          <div className="identities-message">Chargement des identités...</div>
        )}

        {!loading && error && (
          <div className="identities-message error">{error}</div>
        )}

        {!loading && !error && filteredIdentities.length === 0 && (
          <div className="identities-message">
            Aucune identité ne correspond aux critères.
          </div>
        )}

        {!loading && !error && filteredIdentities.length > 0 && (
          <div className="identities-table-container">
            <table className="identities-table">
              <thead>
                <tr>
                  <th>Identité</th>
                  <th>Type</th>
                  <th>État</th>
                  <th>Privilèges</th>
                  <th>Dernière connexion</th>
                  <th>Propriétaire</th>
                  <th>Collectée le</th>
                </tr>
              </thead>

              <tbody>
                {filteredIdentities.map((identity) => (
                  <tr key={identity.id}>
                    <td>
                      <strong>{identity.displayName}</strong>

                      <span>{identity.userName}</span>

                      {identity.description && (
                        <small>{identity.description}</small>
                      )}
                    </td>

                    <td>
                      <span
                        className={`account-type-badge ${identity.accountType.toLowerCase()}`}
                      >
                        {getAccountTypeLabel(identity.accountType)}
                      </span>
                    </td>

                    <td>
                      <div className="identity-state-badges">
                        <span
                          className={`identity-state ${
                            identity.isEnabled ? "enabled" : "disabled"
                          }`}
                        >
                          {identity.isEnabled ? "Actif" : "Désactivé"}
                        </span>

                        {identity.isLocked === true && (
                          <span className="identity-state locked">
                            Verrouillé
                          </span>
                        )}
                      </div>
                    </td>

                    <td>
                      <span
                        className={`privilege-badge ${
                          identity.isPrivileged ? "privileged" : "standard"
                        }`}
                      >
                        {identity.isPrivileged ? "Privilégié" : "Standard"}
                      </span>
                    </td>

                    <td>{formatDate(identity.lastSignInAt)}</td>

                    <td>{identity.owner ?? "Non renseigné"}</td>

                    <td>{formatDate(identity.collectedAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  );
}

export default AuditIdentitiesPanel;
