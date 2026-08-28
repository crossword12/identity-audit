import axios from 'axios'
import {
  clearAuthSession,
  getAccessToken,
  notifyAuthSessionExpired,
} from '../auth/authStorage'

const apiBaseUrl =
  import.meta.env.VITE_API_BASE_URL ??
  'http://localhost:5173'

const httpClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
})

httpClient.interceptors.request.use((config) => {
  const accessToken = getAccessToken()

  if (accessToken) {
    config.headers.set(
      'Authorization',
      `Bearer ${accessToken}`,
    )
  }

  return config
})

httpClient.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (
      axios.isAxiosError(error) &&
      error.response?.status === 401 &&
      error.config?.url !== '/api/auth/login'
    ) {
      clearAuthSession()
      notifyAuthSessionExpired()
    }

    return Promise.reject(error)
  },
)

export default httpClient