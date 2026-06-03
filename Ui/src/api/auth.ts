import apiClient from './apiClient'

export interface AdminLoginRequest {
  email: string
  password: string
}

export interface AdminLoginResponse {
  token: string
  userType: string
  clientId?: number
  name: string
  emailId: string
  expiration: string
  [key: string]: unknown
}


export async function loginAdmin(data: AdminLoginRequest): Promise<AdminLoginResponse> {
  const response = await apiClient.post<AdminLoginResponse>('/Auth/client-login', data)
  return response.data
}

