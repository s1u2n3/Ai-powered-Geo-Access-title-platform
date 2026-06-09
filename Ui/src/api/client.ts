import apiClient from './apiClient'
import type { AdminLoginResponse } from './auth'

export interface County {
  countyCode: string
  countyName: string
  countyLatitude: number
  countyLongitude: number
}

export interface GeoAccess {
  stateCode: string
  stateName: string
  stateLatitude: number
  stateLongitude: number
  counties: County[]
}

export interface Branch {
  branchId: number
  branchCode: string
  branchName: string
  branchType: string
  canLoadStarter: boolean
  canReviewTitle: boolean
  canIssueInsurance: boolean
  geoAccess: GeoAccess[]
}

export interface Company {
  companyId: number
  companyName: string
  branches: Branch[]
}

export interface ClientDetails {
  clientId: number
  clientCode: string
  clientName: string
  clientType: string
  email: string
  phoneNumber: string
  companies: Company[]
}

export interface CreateClientBranch {
  branchCode: string
  canLoadStarter: boolean
  canReviewTitle: boolean
  canIssueInsurance: boolean
}

export interface CreateClientCompany {
  companyName: string
  branches: CreateClientBranch[]
}

export interface CreateClientPayload {
  clientCode: string
  clientName: string
  clientType: string
  email: string
  phoneNumber: string
  companies: CreateClientCompany[]
}
export interface GetAllCompany{
  id: number
  name: string
}
export interface GetBrancheesByCompanyName{
  id:number
  branchCode: string
}

export async function fetchClientDetails(): Promise<ClientDetails> {
  const response = await apiClient.get<ClientDetails>('/Client/client-details')
  return response.data
}

export async function createClient(payload: CreateClientPayload): Promise<ClientDetails> {
  const response = await apiClient.post<ClientDetails>('/Client', payload)
  return response.data
}

export async function GetAllCompany(): Promise<GetAllCompany[]> {
  const response = await apiClient.get<GetAllCompany[]>('/Company')
  return response.data
}

export async function GetCompanyDetails(companyName: string): Promise<Company> {
  const response = await apiClient.get<Company>(`/Company/${encodeURIComponent(companyName)}`)
  return response.data
}
export async function GetBrancheesByCompanyName(companyName: string): Promise<GetBrancheesByCompanyName[]> {
  const response = await apiClient.get<GetBrancheesByCompanyName[]>(`/Branch/${encodeURIComponent(companyName)}`)
  return response.data
}

export interface ActivateClientPayload {
  token: string
  password: string
  confirmPassword: string
}

export async function activateClient(payload: ActivateClientPayload): Promise<AdminLoginResponse> {
  const response = await apiClient.post<AdminLoginResponse>('/Auth/activate-client', payload)
  return response.data
}