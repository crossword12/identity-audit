export type ApplicationRole =
  | 'Administrator'
  | 'Auditor'
  | 'Reader'

export interface LoginRequest {
  email: string
  password: string
}

export interface AuthenticatedUser {
  id: string
  email: string
  displayName: string
  roles: ApplicationRole[]
}

export interface LoginResponse {
  accessToken: string
  tokenType: string
  expiresAt: string
  user: AuthenticatedUser
}

export interface AuthSession {
  accessToken: string
  expiresAt: string
  user: AuthenticatedUser
}