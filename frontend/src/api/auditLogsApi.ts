import httpClient from "./httpClient";
import type { AuditLog } from "../types/auditLog";

export async function getAuditLogs(
  auditId: string,
): Promise<AuditLog[]> {
  const response = await httpClient.get<AuditLog[]>(
    `/api/audits/${auditId}/logs`,
  );

  return response.data;
}

export async function getGlobalAuditLogs(
  limit = 200,
): Promise<AuditLog[]> {
  const response = await httpClient.get<AuditLog[]>(
    "/api/audit-logs",
    {
      params: {
        limit,
      },
    },
  );

  return response.data;
}