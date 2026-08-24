export type TargetType = 'EntraId' | 'ActiveDirectory'

export interface Target {
  id: string
  name: string
  type: TargetType
  isEnabled: boolean
  configurationJson: string | null
  createdAt: string
  updatedAt: string
  lastCollectedAt: string | null
}

export interface CreateTargetRequest {
  name: string
  type: TargetType
  isEnabled: boolean
  configurationJson: string | null
}

export interface UpdateTargetRequest {
  name: string
  isEnabled: boolean
  configurationJson: string | null
}

export interface TestTargetConnectionResult {
  succeeded: boolean
  message: string
  testedAt: string
}