import { Navigate, Outlet } from "react-router-dom";
import type { ApplicationRole } from "../types/auth";
import { hasAnyRole } from "./authorization";
import { useAuth } from "./useAuth";

interface RequireRoleProps {
  allowedRoles: readonly ApplicationRole[];
}

function RequireRole({ allowedRoles }: RequireRoleProps) {
  const { user } = useAuth();

  if (!hasAnyRole(user, allowedRoles)) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}

export default RequireRole;
