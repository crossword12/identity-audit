import type {
  ApplicationRole,
  AuthenticatedUser,
} from "../types/auth";

export const ApplicationRoles = {
  Administrator: "Administrator",
  Auditor: "Auditor",
  Reader: "Reader",
} as const;

export function hasAnyRole(
  user: AuthenticatedUser | null,
  roles: readonly ApplicationRole[],
): boolean {
  if (!user) {
    return false;
  }

  return roles.some((role) => user.roles.includes(role));
}

export function canReadAuditData(
  user: AuthenticatedUser | null,
): boolean {
  return hasAnyRole(user, [
    ApplicationRoles.Reader,
    ApplicationRoles.Auditor,
    ApplicationRoles.Administrator,
  ]);
}

export function canManageAudits(
  user: AuthenticatedUser | null,
): boolean {
  return hasAnyRole(user, [
    ApplicationRoles.Auditor,
    ApplicationRoles.Administrator,
  ]);
}

export function canManageTargets(
  user: AuthenticatedUser | null,
): boolean {
  return hasAnyRole(user, [
    ApplicationRoles.Administrator,
  ]);
}

export function canManageUsers(
  user: AuthenticatedUser | null,
): boolean {
  return hasAnyRole(user, [
    ApplicationRoles.Administrator,
  ]);
}