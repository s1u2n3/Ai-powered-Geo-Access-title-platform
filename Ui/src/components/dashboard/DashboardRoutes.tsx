import { Navigate, Route, Routes } from 'react-router-dom'
import { ClientProvider } from '../../contexts'
import DashboardShell from './DashboardShell'
import {
  ClientDashBoard,
  CreateClientUsers,
  DocumentChunk,
  Documents,
  GeoAccess,
  LoadFullStarter,
  Profile,
  PropertySearch,
  StarterWorkspace,
  
} from './tabs'
import type { AdminLoginResponse } from '../../api/auth'

interface DashboardRoutesProps {
  auth: AdminLoginResponse
  onLogout: () => void
}

export default function DashboardRoutes({ auth, onLogout }: DashboardRoutesProps) {
  const isAdminUser = auth.userType?.toLowerCase() === 'user'
  const fallbackPath = isAdminUser ? '/admin/create-client-users' : '/client/clientdashboard'

  return (
    <ClientProvider>
      <Routes>
        {isAdminUser ? (
          <Route path="/admin/*" element={<DashboardShell auth={auth} onLogout={onLogout} />}>
            <Route index element={<Navigate to="create-client-users" replace />} />
            <Route path="create-client-users" element={<CreateClientUsers />} />
            <Route path="document-chunk" element={<DocumentChunk />} />
            <Route path="load-full-starter" element={<LoadFullStarter />} />
          </Route>
        ) : (
          <Route path="/client/*" element={<DashboardShell auth={auth} onLogout={onLogout} />}>
            <Route index element={<Navigate to="clientdashboard" replace />} />
            <Route path="clientdashboard" element={<ClientDashBoard />} />
            <Route path="property" element={<PropertySearch />} />
            <Route path="workspace" element={<StarterWorkspace />} />
            <Route path="documents" element={<Documents />} />
            <Route path="geo" element={<GeoAccess />} />
            <Route path="profile" element={<Profile />} />
          </Route>
        )}
        <Route path="*" element={<Navigate to={fallbackPath} replace />} />
      </Routes>
    </ClientProvider>
  )
}
