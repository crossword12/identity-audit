import httpClient from "./httpClient";
import type { Audit, CreateAuditRequest } from "../types/audit";

export async function getAudits(): Promise<Audit[]> {
  const response = await httpClient.get<Audit[]>("/api/audits");
  return response.data;
}

export async function getAuditById(id: string): Promise<Audit> {
  const response = await httpClient.get<Audit>(`/api/audits/${id}`);
  return response.data;
}

export async function createAudit(
  request: CreateAuditRequest,
): Promise<Audit> {
  const response = await httpClient.post<Audit>(
    "/api/audits",
    request,
  );

  return response.data;
}

export async function startAudit(id: string): Promise<Audit> {
  const response = await httpClient.post<Audit>(
    `/api/audits/${id}/start`,
  );

  return response.data;
}

export async function completeAudit(id: string): Promise<Audit> {
  const response = await httpClient.post<Audit>(
    `/api/audits/${id}/complete`,
  );

  return response.data;
}