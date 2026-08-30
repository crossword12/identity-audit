export type AuditLogLevel =
  | "Information"
  | "Warning"
  | "Error";

export interface AuditLog {
  id: string;
  auditId: string | null;
  applicationUserId: string | null;
  applicationUserDisplayName: string | null;
  applicationUserEmail: string | null;
  level: AuditLogLevel;
  eventType: string;
  message: string;
  createdAt: string;
}