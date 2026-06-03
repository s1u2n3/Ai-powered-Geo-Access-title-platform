import React, { useEffect } from 'react'
import { NavLink, Outlet, useLocation } from 'react-router-dom'
import {
  AppBar,
  Box,
  Button,
  CssBaseline,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import MenuIcon from '@mui/icons-material/Menu'
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft'
import SearchIcon from '@mui/icons-material/Search'
import WorkspacePremiumIcon from '@mui/icons-material/WorkspacePremium'
import DescriptionIcon from '@mui/icons-material/Description'
import PublicIcon from '@mui/icons-material/Public'
import PersonIcon from '@mui/icons-material/Person'
import LogoutIcon from '@mui/icons-material/Logout'
import AccountBoxIcon from '@mui/icons-material/AccountBox';
import { useClient } from '../../contexts'
import { fetchClientDetails } from '../../api/client'
import type { AdminLoginResponse } from '../../api/auth'

const drawerWidth = 280

const tabs = [
  { path: '/client/clientdashboard', label: 'Client Dashboard', icon: <AccountBoxIcon /> },
  { path: '/client/property', label: 'Property Search', icon: <SearchIcon /> },
  { path: '/client/workspace', label: 'Starter Workspace', icon: <WorkspacePremiumIcon /> },
  { path: '/client/documents', label: 'Documents', icon: <DescriptionIcon /> },
  { path: '/client/geo', label: 'Geo Access', icon: <PublicIcon /> },
  { path: '/client/profile', label: 'Profile', icon: <PersonIcon /> },
]

interface DashboardShellProps {
  auth: AdminLoginResponse
  onLogout: () => void
}

function DashboardShell({ auth, onLogout }: DashboardShellProps) {
  const theme = useTheme()
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'))
  const location = useLocation()
  const isPropertySearch = location.pathname === '/client/property'
  const currentTab = tabs.find((tab) => tab.path === location.pathname)
  const [mobileOpen, setMobileOpen] = React.useState(false)
  const { setClientDetails, setLoadingDetails, setSelectedCompanyId, setSelectedBranchId } =
    useClient()

  // Fetch client details on mount
  useEffect(() => {
    const loadClientDetails = async () => {
      try {
        const details = await fetchClientDetails()
        setClientDetails(details)
        // Set default selections
        if (details.companies.length > 0) {
          setSelectedCompanyId(details.companies[0].companyId)
          if (details.companies[0].branches.length > 0) {
            setSelectedBranchId(details.companies[0].branches[0].branchId)
          }
        }
      } catch (error) {
        console.error('Failed to load client details:', error)
      } finally {
        setLoadingDetails(false)
      }
    }

    loadClientDetails()
  }, [setClientDetails, setLoadingDetails, setSelectedCompanyId, setSelectedBranchId])

  const drawerContent = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ p: 3, pb: 2, background: 'linear-gradient(180deg, #1840c1 0%, #ffffff 100%)' }}>
        <Typography component="div" variant="button" color="primary" sx={{ fontWeight: 700, mb: 0.5 }}>
          Client Portal
        </Typography>
        <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }}>
          Secure Title Processing LLC
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          A clean workspace for your access control workflows.
        </Typography>
      </Box>

      <Divider />

      <List sx={{ flexGrow: 1 }}>
        {tabs.map((tab) => (
          <ListItemButton
            key={tab.path}
            component={NavLink}
            to={tab.path}
            selected={location.pathname === tab.path}
            sx={{
              '&.Mui-selected': {
                bgcolor: 'rgba(79, 70, 229, 0.12)',
                color: 'primary.main',
              },
            }}
          >
            <ListItemIcon sx={{ color: 'inherit' }}>{tab.icon}</ListItemIcon>
            <ListItemText primary={tab.label} />
          </ListItemButton>
        ))}
      </List>

      <Divider />

      <Box sx={{ p: 3 }}>
        <Button
          fullWidth
          variant="outlined"
          startIcon={<LogoutIcon />}
          onClick={onLogout}
        >
          Sign out
        </Button>
      </Box>
    </Box>
  )

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: '#f4f7ff' }}>
      <CssBaseline />
      <AppBar
        position="fixed"
        elevation={0}
        sx={{
          width: { md: `calc(100% - ${drawerWidth}px)` },
          ml: { md: `${drawerWidth}px` },
          background: 'rgba(255,255,255,0.92)',
          color: '#111827',
          backdropFilter: 'blur(12px)',
          borderBottom: '1px solid rgba(15, 23, 42, 0.08)',
        }}
      >
        <Toolbar sx={{ justifyContent: 'space-between', minHeight: 84 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {!isDesktop && (
              <IconButton edge="start" color="inherit" onClick={() => setMobileOpen(true)}>
                <MenuIcon />
              </IconButton>
            )}
            <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }} noWrap>
              {currentTab?.label || (auth.userType === 'Client' ? 'Client Dashboard' : 'Admin Dashboard')}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography color="text.secondary" variant="body2">
              Signed in as {auth.emailId}
            </Typography>          
          </Box>
        </Toolbar>
      </AppBar>

      <Box component="nav" sx={{ width: { md: drawerWidth }, flexShrink: { md: 0 } }}>
        {isDesktop ? (
          <Drawer
            variant="permanent"
            open
            sx={{
              '& .MuiDrawer-paper': {
                width: drawerWidth,
                boxSizing: 'border-box',
                borderRight: '1px solid rgba(15, 23, 42, 0.08)',
                boxShadow: '0 50px 120px rgba(15,23,42,0.08)',
              },
            }}
          >
            {drawerContent}
          </Drawer>
        ) : (
          <Drawer
            variant="temporary"
            open={mobileOpen}
            onClose={() => setMobileOpen(false)}
            ModalProps={{ keepMounted: true }}
            sx={{
              '& .MuiDrawer-paper': {
                width: drawerWidth,
                boxSizing: 'border-box',
              },
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', px: 2, py: 1, justifyContent: 'space-between' }}>
              <Typography component="div" variant="subtitle1" sx={{ fontWeight: 700 }}>
                Menu
              </Typography>
              <IconButton onClick={() => setMobileOpen(false)}>
                <ChevronLeftIcon />
              </IconButton>
            </Box>
            {drawerContent}
          </Drawer>
        )}
      </Box>

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: isPropertySearch ? 0 : 4,
          width: { md: `calc(100% - ${drawerWidth}px)` },
        }}
      >
        <Toolbar />
        {/* Hide the welcome banner on the Property Search page to match design */}
        {!isPropertySearch && (
          <Box
            sx={{
              mb: 3,
              p: 4,
              borderRadius: 3,
              background: 'linear-gradient(180deg, rgba(255,255,255,0.96), rgba(248,250,255,0.96))',
              boxShadow: '0 30px 80px rgba(15,23,42,0.08)',
            }}
          >
            <Typography component="h1" variant="h4" sx={{ fontWeight: 700, mb: 1 }}>
              Welcome back, {auth.name}
            </Typography>
            <Typography color="text.secondary">
              Your client portal gives you immediate access to property search, workspace collaboration, documents, and geo access.
            </Typography>
          </Box>
        )}

        <Box
          sx={{
            minHeight: '62vh',
            p: isPropertySearch ? 0 : 4,
            borderRadius: isPropertySearch ? 0 : 4,
            background: isPropertySearch ? 'transparent' : '#ffffff',
            boxShadow: isPropertySearch ? 'none' : '0 40px 100px rgba(15,23,42,0.08)',
          }}
        >
          <Outlet />
        </Box>
      </Box>
    </Box>
  )
}

export default DashboardShell
