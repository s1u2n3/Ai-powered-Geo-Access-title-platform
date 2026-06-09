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
import GroupAddIcon from '@mui/icons-material/GroupAdd'
import ContentCutIcon from '@mui/icons-material/ContentCut'
import UploadFileIcon from '@mui/icons-material/UploadFile'
import { useClient } from '../../contexts'
import { fetchClientDetails } from '../../api/client'
import type { AdminLoginResponse } from '../../api/auth'

const drawerWidth = 280

const clientTabs = [
  { path: '/client/clientdashboard', label: 'Client Dashboard', icon: <AccountBoxIcon /> },
  { path: '/client/property', label: 'Property Search', icon: <SearchIcon /> },
  { path: '/client/workspace', label: 'Starter Workspace', icon: <WorkspacePremiumIcon /> },
  { path: '/client/documents', label: 'Documents', icon: <DescriptionIcon /> },
  { path: '/client/geo', label: 'Geo Access', icon: <PublicIcon /> },
  { path: '/client/profile', label: 'Profile', icon: <PersonIcon /> },
]

const adminTabs = [
  { path: '/admin/create-client-users', label: 'Create Client Users', icon: <GroupAddIcon /> },
  { path: '/admin/document-chunk', label: 'Document Chunk', icon: <ContentCutIcon /> },
  { path: '/admin/load-full-starter', label: 'Load Full Starter', icon: <UploadFileIcon /> },
]

interface DashboardShellProps {
  auth: AdminLoginResponse
  onLogout: () => void
}

function DashboardShell({ auth, onLogout }: DashboardShellProps) {
  const theme = useTheme()
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'))
  const location = useLocation()
  const isAdminUser = auth.userType?.toLowerCase() === 'user'
  const activeTabs = isAdminUser ? adminTabs : clientTabs
  const isPropertySearch = location.pathname === '/client/property'
  const currentTab = activeTabs.find((tab) => tab.path === location.pathname)
  const [mobileOpen, setMobileOpen] = React.useState(false)
  const { setClientDetails, setLoadingDetails, setSelectedCompanyId, setSelectedBranchId } =
    useClient()

  // Fetch client details on mount
  useEffect(() => {
    if (isAdminUser) {
      setLoadingDetails(false)
      return
    }

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
  }, [isAdminUser, setClientDetails, setLoadingDetails, setSelectedCompanyId, setSelectedBranchId])

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
        {clientTabs.map((tab) => (
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
          width: { md: isAdminUser ? '100%' : `calc(100% - ${drawerWidth}px)` },
          ml: { md: isAdminUser ? 0 : `${drawerWidth}px` },
          background: 'rgba(255,255,255,0.92)',
          color: '#111827',
          backdropFilter: 'blur(12px)',
          borderBottom: '1px solid rgba(15, 23, 42, 0.08)',
        }}
      >
        <Toolbar sx={{ justifyContent: 'space-between', minHeight: isAdminUser ? 58 : 84, gap: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25, flexShrink: 0 }}>
            {!isDesktop && !isAdminUser && (
              <IconButton edge="start" color="inherit" onClick={() => setMobileOpen(true)}>
                <MenuIcon />
              </IconButton>
            )}
            {isAdminUser ? (
              <>
                <Box
                  sx={{
                    width: 30,
                    height: 30,
                    borderRadius: 2,
                    display: 'grid',
                    placeItems: 'center',
                    color: '#ffffff',
                    background: 'linear-gradient(135deg, #635bff, #7c3aed)',
                  }}
                >
                  <GroupAddIcon sx={{ fontSize: 18 }} />
                </Box>
                <Typography component="h2" variant="subtitle1" sx={{ fontWeight: 900 }} noWrap>
                  GeoAccess
                </Typography>
              </>
            ) : (
              <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }} noWrap>
                {currentTab?.label || 'Client Dashboard'}
              </Typography>
            )}
          </Box>

          {isAdminUser && (
            <Box
              sx={{
                display: { xs: 'none', md: 'flex' },
                alignItems: 'center',
                gap: 0,
                flex: 1,
                alignSelf: 'stretch',
              }}
            >
              {adminTabs.map((tab) => (
                <Button
                  key={tab.path}
                  component={NavLink}
                  to={tab.path}
                  startIcon={tab.icon}
                  sx={{
                    color: location.pathname === tab.path ? 'primary.main' : 'text.secondary',
                    bgcolor: location.pathname === tab.path ? 'rgba(99, 91, 255, 0.14)' : 'transparent',
                    borderRadius: 0,
                    minWidth: 150,
                    fontWeight: 800,
                    px: 2,
                    borderLeft: '1px solid rgba(15, 23, 42, 0.06)',
                    borderRight: '1px solid rgba(15, 23, 42, 0.03)',
                    '&:hover': {
                      bgcolor: 'rgba(99, 91, 255, 0.1)',
                    },
                  }}
                >
                  {tab.label}
                </Button>
              ))}
            </Box>
          )}

          <Box sx={{ display: 'flex', alignItems: 'center', gap: isAdminUser ? 1.5 : 2, flexShrink: 0 }}>
            {isAdminUser && (
              <Box
                sx={{
                  display: { xs: 'none', lg: 'block' },
                  border: '1px solid rgba(15, 23, 42, 0.1)',
                  borderRadius: 1.5,
                  px: 2,
                  py: 0.75,
                  minWidth: 180,
                }}
              >
                <Typography variant="caption" color="text.secondary" sx={{ display: 'block', lineHeight: 1 }}>
                  Workspace
                </Typography>
                <Typography variant="caption" sx={{ fontWeight: 800, color: '#0f172a' }}>
                  Super Admin Workspace
                </Typography>
              </Box>
            )}
            <Box sx={{ display: { xs: 'none', sm: 'block' } }}>
              <Typography color="text.secondary" variant="caption" sx={{ display: 'block', lineHeight: 1 }}>
                Signed in as
              </Typography>
              <Typography variant="caption" sx={{ fontWeight: 800, color: '#0f172a' }}>
                {auth.emailId}
              </Typography>
            </Box>
            {isAdminUser && (
              <Box sx={{ position: 'relative', width: 28, height: 28, borderRadius: '50%', bgcolor: '#ede9fe', color: '#6d28d9', display: 'grid', placeItems: 'center', fontWeight: 900, fontSize: 13 }}>
                {auth.name?.charAt(0) || 'A'}
                <Box sx={{ position: 'absolute', right: -1, bottom: 0, width: 8, height: 8, borderRadius: '50%', bgcolor: '#22c55e', border: '2px solid #ffffff' }} />
              </Box>
            )}
            <Button variant="outlined" size="small" startIcon={<LogoutIcon />} onClick={onLogout} sx={{ fontWeight: 800 }}>
              Sign out
            </Button>
          </Box>
        </Toolbar>
        {isAdminUser && (
          <Box
            sx={{
              display: { xs: 'flex', md: 'none' },
              gap: 1,
              px: 2,
              pb: 1.5,
              overflowX: 'auto',
              borderTop: '1px solid rgba(15, 23, 42, 0.06)',
            }}
          >
            {adminTabs.map((tab) => (
              <Button
                key={tab.path}
                component={NavLink}
                to={tab.path}
                startIcon={tab.icon}
                size="small"
                sx={{
                  flexShrink: 0,
                  color: location.pathname === tab.path ? 'primary.main' : 'text.secondary',
                  bgcolor: location.pathname === tab.path ? 'rgba(79, 70, 229, 0.12)' : 'transparent',
                  fontWeight: 800,
                }}
              >
                {tab.label}
              </Button>
            ))}
          </Box>
        )}
      </AppBar>

      {!isAdminUser && (
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
      )}

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: isAdminUser ? 0 : isPropertySearch ? 0 : 4,
          width: { md: isAdminUser ? '100%' : `calc(100% - ${drawerWidth}px)` },
        }}
      >
        <Toolbar />
        {isAdminUser && <Toolbar sx={{ display: { xs: 'block', md: 'none' }, minHeight: 36 }} />}
        {/* Hide the welcome banner on the Property Search page to match design */}
        {!isAdminUser && !isPropertySearch && (
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
              {isAdminUser
                ? 'Your admin portal gives you access to client users, document chunking, full starter loading, and workspace review.'
                : 'Your client portal gives you immediate access to property search, workspace collaboration, documents, and geo access.'}
            </Typography>
          </Box>
        )}

        <Box
          sx={{
            minHeight: '62vh',
            p: isAdminUser ? 0 : isPropertySearch ? 0 : 4,
            borderRadius: isAdminUser || isPropertySearch ? 0 : 4,
            background: isAdminUser || isPropertySearch ? 'transparent' : '#ffffff',
            boxShadow: isAdminUser || isPropertySearch ? 'none' : '0 40px 100px rgba(15,23,42,0.08)',
          }}
        >
          <Outlet />
        </Box>
      </Box>
    </Box>
  )
}

export default DashboardShell
