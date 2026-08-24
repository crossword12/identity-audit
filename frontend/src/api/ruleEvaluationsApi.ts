import httpClient from "./httpClient";
import type {
  EvaluateAuditRulesResult,
  RuleEvaluation,
} from "../types/ruleEvaluation";

export async function getAuditRuleEvaluations(
  auditId: string,
): Promise<RuleEvaluation[]> {
  const response = await httpClient.get<RuleEvaluation[]>(
    `/api/audits/${auditId}/rule-evaluations`,
  );

  return response.data;
}

export async function evaluateAuditRules(
  auditId: string,
): Promise<EvaluateAuditRulesResult> {
  const response =
    await httpClient.post<EvaluateAuditRulesResult>(
      `/api/audits/${auditId}/evaluate`,
    );

  return response.data;
}