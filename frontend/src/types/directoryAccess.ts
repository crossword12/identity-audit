import type { TargetType } from "./target";

export interface DirectoryGroup {
  id: string;
  auditId: string;
  externalId: string;
  name: string;
  description: string | null;
  source: TargetType;
  groupType: string | null;
  isPrivileged: boolean;
  collectedAt: string;
}

export interface GroupMembership {
  id: string;
  auditId: string;
  identityId: string;
  identityExternalId: string;
  identityDisplayName: string;
  groupId: string;
  groupExternalId: string;
  groupName: string;
  membershipType: string;
  collectedAt: string;
}

export interface DirectoryRole {
  id: string;
  auditId: string;
  externalId: string;
  name: string;
  description: string | null;
  source: string;
  isPrivileged: boolean;
  collectedAt: string;
}

export interface RoleAssignment {
  id: string;
  auditId: string;
  identityId: string;
  identityExternalId: string;
  identityDisplayName: string;
  directoryRoleId: string;
  roleExternalId: string;
  roleName: string;
  roleIsPrivileged: boolean;
  assignedAt: string | null;
  expiresAt: string | null;
  isPermanent: boolean;
  collectedAt: string;
}