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
export interface AuditCsvExport {
  content: Blob;
  fileName: string;
}

function getExportFileName(
  contentDisposition: string | undefined,
  auditId: string,
): string {
  const utf8FileName = contentDisposition?.match(
    /filename\*=UTF-8''([^;]+)/i,
  )?.[1];

  if (utf8FileName) {
    try {
      return decodeURIComponent(utf8FileName.trim());
    } catch {
      // Utilisation du nom simple ci-dessous.
    }
  }

  const simpleFileName = contentDisposition?.match(
    /filename="?([^";]+)"?/i,
  )?.[1];

  return (
    simpleFileName?.trim() ??
    `audit-${auditId}-resultats-cis.csv`
  );
}

export async function exportAuditCsv(
  id: string,
): Promise<AuditCsvExport> {
  const response = await httpClient.get<Blob>(
    `/api/audits/${id}/export.csv`,
    {
      responseType: "blob",
      headers: {
        Accept: "text/csv",
      },
    },
  );

  return {
    content: response.data,
    fileName: getExportFileName(
      response.headers["content-disposition"],
      id,
    ),
  };
}