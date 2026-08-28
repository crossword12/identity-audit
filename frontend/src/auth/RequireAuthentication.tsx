import type { CSSProperties } from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { ShieldCheck } from "lucide-react";
import { useAuth } from "./useAuth";

const loadingScreenStyle: CSSProperties = {
  minHeight: "100vh",
  display: "grid",
  placeItems: "center",
  gap: "12px",
  background: "#f5f7fa",
  color: "#1f4e78",
  fontWeight: 600,
};

function RequireAuthentication() {
  const { isAuthenticated, isLoading } = useAuth();

  const location = useLocation();

  if (isLoading) {
    return (
      <div role="status" aria-live="polite" style={loadingScreenStyle}>
        <ShieldCheck size={36} />
        <span>Vérification de la session...</span>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}

export default RequireAuthentication;
