export interface DashboardAudit {
  auditId: string
  targetId: string
  targetName: string
  targetType: string
  auditStatus: string
  createdAt: string
  startedAt: string | null
  completedAt: string | null
  complianceScore: number | null

  totalIdentities: number
  enabledIdentities: number
  disabledIdentities: number
  privilegedIdentities: number
  serviceAccounts: number
  guestAccounts: number
  lockedAccounts: number

  evaluatedRules: number
  compliantRules: number
  nonCompliantRules: number
  notApplicableRules: number
  notVerifiableRules: number
  errorRules: number

  totalFindings: number
  criticalFindings: number
  highFindings: number
  mediumFindings: number
  lowFindings: number
  cisControl5Findings: number
  cisControl6Findings: number
}