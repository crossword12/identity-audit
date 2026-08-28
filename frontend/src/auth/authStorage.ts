import type {
  ApplicationRole,
  AuthSession,
  AuthenticatedUser,
} from '../types/auth'

const AUTH_SESSION_KEY = 'identity-audit.auth-session'

const EXPIRATION_MARGIN_MILLISECONDS = 30_000

export const AUTH_SESSION_EXPIRED_EVENT =
  'identity-audit:auth-session-expired'

const allowedRoles: readonly ApplicationRole[] = [
  'Administrator',
  'Auditor',
  'Reader',
]

function isApplicationRole(
  value: unknown,
): value is ApplicationRole {
  return (
    typeof value === 'string' &&
    allowedRoles.includes(value as ApplicationRole)
  )
}

function isAuthenticatedUser(
  value: unknown,
): value is AuthenticatedUser {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const user = value as Record<string, unknown>

  return (
    typeof user.id === 'string' &&
    typeof user.email === 'string' &&
    typeof user.displayName === 'string' &&
    Array.isArray(user.roles) &&
    user.roles.every(isApplicationRole)
  )
}

function isAuthSession(
  value: unknown,
): value is AuthSession {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const session = value as Record<string, unknown>

  return (
    typeof session.accessToken === 'string' &&
    session.accessToken.length > 0 &&
    typeof session.expiresAt === 'string' &&
    isAuthenticatedUser(session.user)
  )
}

export function isAuthSessionExpired(
  session: AuthSession,
): boolean {
  const expiresAt =
    Date.parse(session.expiresAt)

  if (Number.isNaN(expiresAt)) {
    return true
  }

  return (
    expiresAt <=
    Date.now() + EXPIRATION_MARGIN_MILLISECONDS
  )
}

export function getAuthSession(): AuthSession | null {
  const serializedSession =
    sessionStorage.getItem(AUTH_SESSION_KEY)

  if (!serializedSession) {
    return null
  }

  try {
    const session: unknown =
      JSON.parse(serializedSession)

    if (
      !isAuthSession(session) ||
      isAuthSessionExpired(session)
    ) {
      clearAuthSession()
      return null
    }

    return session
  } catch {
    clearAuthSession()
    return null
  }
}

export function setAuthSession(
  session: AuthSession,
): void {
  sessionStorage.setItem(
    AUTH_SESSION_KEY,
    JSON.stringify(session),
  )
}

export function clearAuthSession(): void {
  sessionStorage.removeItem(AUTH_SESSION_KEY)
}

export function getAccessToken(): string | null {
  return getAuthSession()?.accessToken ?? null
}

export function notifyAuthSessionExpired(): void {
  window.dispatchEvent(
    new Event(AUTH_SESSION_EXPIRED_EVENT),
  )
}