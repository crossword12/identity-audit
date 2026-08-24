import httpClient from './httpClient'
import type { DashboardAudit } from '../types/dashboard'

export async function getTargetDashboard(
  targetId: string,
): Promise<DashboardAudit[]> {
  const response = await httpClient.get<DashboardAudit[]>(
    `/api/dashboard/targets/${targetId}`,
  )

  return response.data
}