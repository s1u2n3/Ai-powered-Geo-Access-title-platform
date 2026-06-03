import apiClient from './apiClient'

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

export async function fetchClientDetails(): Promise<ClientDetails> {
  const response = await apiClient.get<ClientDetails>('/Client/client-details')
  return response.data
}
