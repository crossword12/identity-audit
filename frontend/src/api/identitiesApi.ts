import httpClient from "./httpClient";
import type { DirectoryIdentity } from "../types/identity";

export async function getAuditIdentities(
  auditId: string,
): Promise<DirectoryIdentity[]> {
  const response = await httpClient.get<DirectoryIdentity[]>(
    `/api/audits/${auditId}/identities`,
  );

  return response.data;
}