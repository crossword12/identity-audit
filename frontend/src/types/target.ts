export type TargetType = 'EntraId' | 'ActiveDirectory'

export interface Target {
  id: string
  name: string
  type: TargetType
  isEnabled: boolean
  configurationJson: string
  createdAt: string
  updatedAt: string | null
  lastCollectedAt: string | null
}