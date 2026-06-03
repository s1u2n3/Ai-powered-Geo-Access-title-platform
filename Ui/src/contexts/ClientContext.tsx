import { createContext, useState } from 'react'
import type { ReactNode } from 'react'
import type { ClientDetails } from '../api/client'

interface ClientContextType {
  clientDetails: ClientDetails | null
  selectedCompanyId: number | null
  selectedBranchId: number | null
  loadingDetails: boolean
  setSelectedCompanyId: (id: number) => void
  setSelectedBranchId: (id: number) => void
  setClientDetails: (details: ClientDetails | null) => void
  setLoadingDetails: (loading: boolean) => void
}

export const ClientContext = createContext<ClientContextType | undefined>(undefined)

export function ClientProvider({ children }: { children: ReactNode }) {
  const [clientDetails, setClientDetails] = useState<ClientDetails | null>(null)
  const [selectedCompanyId, setSelectedCompanyId] = useState<number | null>(null)
  const [selectedBranchId, setSelectedBranchId] = useState<number | null>(null)
  const [loadingDetails, setLoadingDetails] = useState(true)

  return (
    <ClientContext.Provider
      value={{
        clientDetails,
        selectedCompanyId,
        selectedBranchId,
        loadingDetails,
        setSelectedCompanyId,
        setSelectedBranchId,
        setClientDetails,
        setLoadingDetails,
      }}
    >
      {children}
    </ClientContext.Provider>
  )
}
