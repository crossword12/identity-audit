import { NavLink, Outlet, useNavigate } from "react-router-dom";
import {
  LayoutDashboard,
  LogOut,
  ScanSearch,
  Server,
  ShieldCheck,
  Users,
} from "lucide-react";
import { useAuth } from "../../auth/useAuth";
import type { ApplicationRole } from "../../types/auth";
import { canManageUsers } from "../../auth/authorization";
import "./AppLayout.css";

const roleLabels: Record<ApplicationRole, string> = {
  Administrator: "Administrateur",
  Auditor: "Auditeur",
  Reader: "Lecteur",
};

function getInitials(displayName: string): string {
  return displayName
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();
}

function AppLayout() {
  const { user, logout } = useAuth();
  const mayManageUsers = canManageUsers(user);

  const navigate = useNavigate();

  const displayedRoles =
    user?.roles.map((role) => roleLabels[role]).join(" · ") ?? "";

  function handleLogout() {
    logout();

    navigate("/login", {
      replace: true,
    });
  }

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-icon">
            <ShieldCheck size={25} />
          </div>

          <div>
            <strong>Identity Audit</strong>
            <span>CIS Controls 5 &amp; 6</span>
          </div>
        </div>

        <nav className="navigation">
          <NavLink to="/" end>
            <LayoutDashboard size={20} />
            Tableau de bord
          </NavLink>

          <NavLink to="/targets">
            <Server size={20} />
            Cibles
          </NavLink>

          <NavLink to="/audits">
            <ScanSearch size={20} />
            Audits
          </NavLink>

          {mayManageUsers && (
            <NavLink to="/users">
              <Users size={20} />
              Utilisateurs
            </NavLink>
          )}
        </nav>

        <div className="sidebar-footer">
          <span className="status-dot" />
          API opérationnelle
        </div>
      </aside>

      <div className="main-area">
        <header className="topbar">
          <div>
            <span className="environment-label">ENVIRONNEMENT</span>

            <strong>Audit des identités</strong>
          </div>

          <div className="topbar-actions">
            <div className="security-status">
              <ShieldCheck size={18} />
              Système sécurisé
            </div>

            {user && (
              <div className="user-account">
                <span className="user-avatar">
                  {getInitials(user.displayName)}
                </span>

                <div className="user-identity">
                  <strong>{user.displayName}</strong>

                  <span>{displayedRoles}</span>
                </div>

                <button
                  type="button"
                  className="logout-button"
                  onClick={handleLogout}
                  aria-label="Se déconnecter"
                  title="Se déconnecter"
                >
                  <LogOut size={19} />
                </button>
              </div>
            )}
          </div>
        </header>

        <main className="page-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

export default AppLayout;
