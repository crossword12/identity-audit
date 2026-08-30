import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import RequireAuthentication from "../auth/RequireAuthentication";
import AppLayout from "../components/layout/AppLayout";
import AuditDetailsPage from "../pages/AuditDetails/AuditDetailsPage";
import AuditsPage from "../pages/Audits/AuditsPage";
import DashboardPage from "../pages/Dashboard/DashboardPage";
import LoginPage from "../pages/Login/LoginPage";
import TargetsPage from "../pages/Targets/TargetsPage";
import { ApplicationRoles } from "../auth/authorization";
import RequireRole from "../auth/RequireRole";
import UsersPage from "../pages/Users/UsersPage";
import ActivityLogsPage from "../pages/ActivityLogs/ActivityLogsPage";

function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route element={<RequireAuthentication />}>
          <Route element={<AppLayout />}>
            <Route index element={<DashboardPage />} />

            <Route path="targets" element={<TargetsPage />} />

            <Route path="audits" element={<AuditsPage />} />

            <Route path="audits/:auditId" element={<AuditDetailsPage />} />
            <Route
              element={
                <RequireRole allowedRoles={[ApplicationRoles.Administrator]} />
              }
            >
              <Route path="users" element={<UsersPage />} />
              <Route path="activity-logs" element={<ActivityLogsPage />} />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default AppRoutes;
