import httpClient from './httpClient'
import type {
  CreateTargetRequest,
  Target,
  TestTargetConnectionResult,
  UpdateTargetRequest,
} from '../types/target'

export async function getTargets(): Promise<Target[]> {
  const response = await httpClient.get<Target[]>('/api/targets')
  return response.data
}

export async function getTargetById(id: string): Promise<Target> {
  const response = await httpClient.get<Target>(`/api/targets/${id}`)
  return response.data
}

export async function createTarget(
  request: CreateTargetRequest,
): Promise<Target> {
  const response = await httpClient.post<Target>('/api/targets', request)
  return response.data
}

export async function updateTarget(
  id: string,
  request: UpdateTargetRequest,
): Promise<Target> {
  const response = await httpClient.put<Target>(
    `/api/targets/${id}`,
    request,
  )

  return response.data
}

export async function testTargetConnection(
  id: string,
): Promise<TestTargetConnectionResult> {
  const response = await httpClient.post<TestTargetConnectionResult>(
    `/api/targets/${id}/test-connection`,
  )

  return response.data
}