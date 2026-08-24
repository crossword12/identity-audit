import axios from 'axios'

const apiBaseUrl =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5173'

const httpClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
})

export default httpClient