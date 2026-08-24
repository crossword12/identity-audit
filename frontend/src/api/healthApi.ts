import httpClient from './httpClient'

export interface HealthResponse {
  status: string
  timestamp?: string
}

export async function getApiHealth(): Promise<HealthResponse> {
  const response = await httpClient.get<HealthResponse>('/api/health')
  return response.data
}