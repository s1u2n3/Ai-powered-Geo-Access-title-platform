import apiClient from './apiClient'

export interface GeoJsonPolygon {
  type: 'Polygon'
  coordinates: number[][][]
}

export interface PropertyGeoData {
  id: number
  parcelId: string
  apn: string
  address: string
  countyName: string
  stateName: string
  latitude: number
  longitude: number
  hasStarter: boolean
  starterFromYear: number
  starterToYear: number
  polygon: GeoJsonPolygon
}

export interface StarterPackage {
  id: number
  parcelId: string
  coverageFromYear: number
  coverageToYear: number
  starterType: string
  status: string
  riskScore: number
  riskLevel: string
  currentOwner: string
  insuranceReady: boolean
  createdDate: string
  starterJson: string
}

export interface StarterIssue {
  id: number
  starterPackageId: number
  issueType: string
  description: string
  severity: string
  resolved: boolean
  createdDate: string
  starterPackage?: StarterPackage
}

export interface PropertySearchResult {
  parcel: PropertyGeoData
  starterPackage: StarterPackage | null
  starterIssues: StarterIssue[]
}

export type PropertySearchOption = 'parcelId'

export interface PropertyGeoDataRequest {
  parcelId: string
}

export async function fetchPropertyGeoData(
  request: PropertyGeoDataRequest,
): Promise<PropertySearchResult> {
  const payload: PropertyGeoDataRequest = {
    parcelId: request.parcelId.trim(),
  }

  const response = await apiClient.post<PropertySearchResult>('/geo/search', payload)
  console.log('API Response:', response.data)
  
  return response.data
}
