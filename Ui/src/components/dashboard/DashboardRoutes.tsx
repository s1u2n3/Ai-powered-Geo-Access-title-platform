import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ClientProvider } from '../../contexts'
import DashboardShell from './DashboardShell'
import {
  ClientDashBoard,
  Documents,
  GeoAccess,
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
  return (
    <ClientProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/client" element={<DashboardShell auth={auth} onLogout={onLogout} />}>
            <Route index element={<Navigate to="clientdashboard" replace />} />
            <Route path="clientdashboard" element={<ClientDashBoard />} />
            <Route path="property" element={<PropertySearch />} />
            <Route path="workspace" element={<StarterWorkspace />} />
            <Route path="documents" element={<Documents />} />
            <Route path="geo" element={<GeoAccess />} />
            <Route path="profile" element={<Profile />} />
          </Route>
          <Route path="*" element={<Navigate to="/client/clientdashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </ClientProvider>
  )
}
