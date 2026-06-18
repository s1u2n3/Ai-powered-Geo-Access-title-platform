import apiClient from './apiClient'

export interface DocumentProcessingResponse {
  JobId: string
  Message: string
}

export async function processDocumentChunks(parcelId: string): Promise<DocumentProcessingResponse> {
  const response = await apiClient.get<DocumentProcessingResponse>(`/Document/ChunkDocuments${encodeURIComponent(parcelId)}`)
  return response.data
}
