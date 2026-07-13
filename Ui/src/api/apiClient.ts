import axios from 'axios'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || '/api'

const apiClient = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
})

// Add bearer token from localStorage to all requests
apiClient.interceptors.request.use((config) => {
  const authData = localStorage.getItem('accessControlAuth')
  if (authData) {
    try {
      const parsed = JSON.parse(authData)
      if (parsed.token) {
        config.headers.Authorization = `Bearer ${parsed.token}`
      }
    } catch (e) {
      console.error('Failed to parse auth token:', e)
    }
  }
  return config
})

export default apiClient

export const analyzeParcel = (parcelId: string, question: string) => {
  return apiClient.post('/AI/analyze', { parcelId, question })
}
