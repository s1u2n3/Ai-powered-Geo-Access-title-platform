import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import LoginPage from './components/auth/LoginPage'
import ActivateClient from './components/auth/ActivateClient'
import DashboardRoutes from './components/dashboard/DashboardRoutes'
import { loginAdmin, type AdminLoginResponse } from './api/auth'

const authStorageKey = 'accessControlAuth'

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#4f46e5',
    },
    secondary: {
      main: '#0f172a',
    },
    background: {
      default: '#f8fbff',
      paper: '#ffffff',
    },
  },
  typography: {
    fontFamily: ['Inter', 'system-ui', 'sans-serif'].join(','),
  },
})

function App() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)
  const [auth, setAuth] = useState<AdminLoginResponse | null>(null)

  useEffect(() => {
    const raw = window.localStorage.getItem(authStorageKey)
    if (raw) {
      try {
        const parsed = JSON.parse(raw) as AdminLoginResponse
        setAuth(parsed)
      } catch {
        window.localStorage.removeItem(authStorageKey)
      }
    }
  }, [])

  useEffect(() => {
    if (auth) {
      window.localStorage.setItem(authStorageKey, JSON.stringify(auth))
    } else {
      window.localStorage.removeItem(authStorageKey)
    }
  }, [auth])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (!email.trim() || !password.trim()) {
      setMessage('Please enter both email and password.')
      return
    }

    setLoading(true)
    setMessage('')

    try {
      const response = await loginAdmin({ email, password })
      setAuth(response)
      setMessage('Login successful. Redirecting to your dashboard...')
    } catch (error) {
      const errorMessage =
        error instanceof Error
          ? error.message
          : 'Unexpected error while logging in.'
      setMessage(`Login failed: ${errorMessage}`)
    } finally {
      setLoading(false)
    }
  }

  const logout = () => {
    setAuth(null)
    setEmail('')
    setPassword('')
    setMessage('You have been logged out.')
  }

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={
            !auth ? (
              <LoginPage
                email={email}
                password={password}
                message={message}
                loading={loading}
                onEmailChange={(value) => setEmail(value)}
                onPasswordChange={(value) => setPassword(value)}
                onSubmit={handleSubmit}
              />
            ) : (
              <Navigate to={auth.userType?.toLowerCase() === 'user' ? '/admin/create-client-users' : '/client/clientdashboard'} replace />
            )
          } />
          <Route path="/activate-account" element={<ActivateClient onActivated={(authData) => setAuth(authData)} />} />
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route
            path="*"
            element={
              !auth ? (
                <Navigate to="/login" replace />
              ) : (
                <DashboardRoutes auth={auth} onLogout={logout} />
              )
            }
          />
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  )
}

export default App
