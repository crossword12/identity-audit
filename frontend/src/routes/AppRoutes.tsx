import { BrowserRouter, Route, Routes } from "react-router-dom";
import AppLayout from "../components/layout/AppLayout";
import AuditsPage from "../pages/Audits/AuditsPage";
import DashboardPage from "../pages/Dashboard/DashboardPage";
import TargetsPage from "../pages/Targets/TargetsPage";
import AuditDetailsPage from "../pages/AuditDetails/AuditDetailsPage";

function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="targets" element={<TargetsPage />} />
          <Route path="audits" element={<AuditsPage />} />
          <Route path="audits/:auditId" element={<AuditDetailsPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default AppRoutes;
