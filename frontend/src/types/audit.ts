export type AuditStatus =
  | "Pending"
  | "Running"
  | "Completed"
  | "CompletedWithWarnings"
  | "Failed";

export interface Audit {
  id: string;
  targetId: string;
  targetName: string;
  status: AuditStatus;
  startedAt: string | null;
  completedAt: string | null;
  complianceScore: number | null;
  errorMessage: string | null;
  createdAt: string;
}

export interface CreateAuditRequest {
  targetId: string;
}