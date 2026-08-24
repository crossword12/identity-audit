import type { TargetType } from "./target";

export type AccountType =
  | "User"
  | "Guest"
  | "Administrator"
  | "ServiceAccount";

export interface DirectoryIdentity {
  id: string;
  auditId: string;
  externalId: string;
  displayName: string;
  userName: string;
  email: string | null;
  source: TargetType;
  accountType: AccountType;
  isEnabled: boolean;
  isPrivileged: boolean;
  isServiceAccount: boolean;
  isLocked: boolean | null;
  lastSignInAt: string | null;
  description: string | null;
  owner: string | null;
  collectedAt: string;
}