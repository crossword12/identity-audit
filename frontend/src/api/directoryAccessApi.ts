import httpClient from "./httpClient";
import type {
  DirectoryGroup,
  DirectoryRole,
  GroupMembership,
  RoleAssignment,
} from "../types/directoryAccess";

export async function getAuditGroups(
  auditId: string,
): Promise<DirectoryGroup[]> {
  const response = await httpClient.get<DirectoryGroup[]>(
    `/api/audits/${auditId}/groups`,
  );

  return response.data;
}

export async function getAuditGroupMemberships(
  auditId: string,
): Promise<GroupMembership[]> {
  const response = await httpClient.get<GroupMembership[]>(
    `/api/audits/${auditId}/group-memberships`,
  );

  return response.data;
}

export async function getAuditRoles(
  auditId: string,
): Promise<DirectoryRole[]> {
  const response = await httpClient.get<DirectoryRole[]>(
    `/api/audits/${auditId}/roles`,
  );

  return response.data;
}

export async function getAuditRoleAssignments(
  auditId: string,
): Promise<RoleAssignment[]> {
  const response = await httpClient.get<RoleAssignment[]>(
    `/api/audits/${auditId}/role-assignments`,
  );

  return response.data;
}