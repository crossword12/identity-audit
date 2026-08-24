import type { TargetType } from "./target";

export type RuleEvaluationStatus =
  | "Compliant"
  | "NonCompliant"
  | "NotApplicable"
  | "NotVerifiable"
  | "Error";

export type RuleSeverity =
  | "Low"
  | "Medium"
  | "High"
  | "Critical";

export interface RuleEvaluation {
  id: string;
  auditId: string;
  auditRuleId: string;
  ruleCode: string;
  ruleName: string;
  cisControl: string;
  targetType: TargetType;
  severity: RuleSeverity;
  status: RuleEvaluationStatus;
  findingCount: number;
  evidenceJson: string | null;
  recommendation: string | null;
  errorMessage: string | null;
  evaluatedAt: string;
}

export interface EvaluateAuditRulesResult {
  auditId: string;
  evaluatedRuleCount: number;
  compliantCount: number;
  nonCompliantCount: number;
  notApplicableCount: number;
  notVerifiableCount: number;
  errorCount: number;
  complianceScore: number | null;
  evaluations: RuleEvaluation[];
}