import { NavLink, Outlet } from "react-router-dom";
import { LayoutDashboard, ScanSearch, Server, ShieldCheck } from "lucide-react";
import "./AppLayout.css";

function AppLayout() {
  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-icon">
            <ShieldCheck size={25} />
          </div>

          <div>
            <strong>Identity Audit</strong>
            <span>CIS Controls 5 & 6</span>
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

          <div className="security-status">
            <ShieldCheck size={18} />
            Système sécurisé
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
