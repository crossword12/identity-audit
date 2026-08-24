import httpClient from './httpClient'
import type { Target } from '../types/target'

export async function getTargets(): Promise<Target[]> {
  const response = await httpClient.get<Target[]>('/api/targets')
  return response.data
}