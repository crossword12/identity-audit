import type {
  AuthenticatedUser,
  LoginRequest,
  LoginResponse,
} from '../types/auth'
import httpClient from './httpClient'

export async function login(
  request: LoginRequest,
): Promise<LoginResponse> {
  const response =
    await httpClient.post<LoginResponse>(
      '/api/auth/login',
      request,
    )

  return response.data
}

export async function getCurrentUser():
  Promise<AuthenticatedUser> {
  const response =
    await httpClient.get<AuthenticatedUser>(
      '/api/auth/me',
    )

  return response.data
}