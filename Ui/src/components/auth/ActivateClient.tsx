import LockOutlinedIcon from '@mui/icons-material/LockOutlined'
import VisibilityIcon from '@mui/icons-material/Visibility'
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff'
import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import {
  Avatar,
  Box,
  Button,
  Card,
  CardContent,
  Container,
  Divider,
  IconButton,
  InputAdornment,
  TextField,
  Typography,
  Alert,
  CircularProgress,
} from '@mui/material'
import { activateClient, } from '../../api/client'
import type { AdminLoginResponse } from '../../api/auth'

interface ActivateClientProps {
  onActivated: (auth: AdminLoginResponse) => void
}

function ActivateClient({ onActivated }: ActivateClientProps) {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')

  const [form, setForm] = useState({
    password: '',
    confirmPassword: '',
  })
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    if (!token) {
      setError('Invalid activation link. Token is missing.')
    }
  }, [token])

  const updateField = (field: keyof typeof form, value: string) => {
    setForm((current) => ({ ...current, [field]: value }))
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage('')
    setError('')

    if (!token) {
      setError('Invalid activation link. Token is missing.')
      return
    }

    if (!form.password || !form.confirmPassword) {
      setError('Please enter both password and confirm password.')
      return
    }

    if (form.password !== form.confirmPassword) {
      setError('Passwords do not match.')
      return
    }

    if (form.password.length < 8) {
      setError('Password must be at least 8 characters long.')
      return
    }

    setLoading(true)
    try {
      const authResponse = await activateClient({
        token,
        password: form.password,
        confirmPassword: form.confirmPassword,
      })

      onActivated(authResponse)
      setMessage('Account activated and logged in successfully! Redirecting...')
      setTimeout(() => {
        if (authResponse.userType?.toLowerCase() === 'user') {
          navigate('/admin/create-client-users', { replace: true })
        } else {
          navigate('/client/clientdashboard', { replace: true })
        }
      }, 500)
    } catch (submitError) {
      const submitMessage =
        submitError instanceof Error ? submitError.message : 'Unexpected error while activating account.'
      setError(submitMessage)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'grid',
        placeItems: 'center',
        backgroundImage:
          'linear-gradient(180deg, rgba(15, 23, 42, 0.71), rgba(79, 70, 229, 0.55)), url(https://images.unsplash.com/photo-1515378791036-0648a3ef77b2?auto=format&fit=crop&w=1400&q=80)',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        color: '#fff',
        py: 8,
      }}
    >
      <Container maxWidth="sm">
        <Card sx={{ borderRadius: 4, overflow: 'hidden', boxShadow: '0 30px 80px rgba(15,23,42,0.22)' }}>
          <Box
            sx={{
              minHeight: '240px',
              px: 6,
              py: 5,
              background: 'rgba(15, 23, 42, 0.92)',
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'center',
              gap: 2,
            }}
          >
            <Typography variant="h4" sx={{ color: '#f8fafc', fontWeight: 700 }}>
              Activate Your Account
            </Typography>
            <Typography variant="body1" sx={{ color: 'rgba(248, 250, 252, 0.84)', maxWidth: 520 }}>
              Set up your password to complete account activation and access your property search, documents, and geo controls.
            </Typography>
          </Box>

          <CardContent sx={{ p: 5, background: '#f8fafc' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
              <Avatar sx={{ bgcolor: '#4f46e5' }}>
                <LockOutlinedIcon />
              </Avatar>
              <div>
                <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }}>
                  Secure Password Setup
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Create a strong password to protect your account.
                </Typography>
              </div>
            </Box>

            {(message || error) && (
              <Alert severity={message ? 'success' : 'error'} sx={{ mb: 3 }}>
                {message || error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleSubmit} noValidate sx={{ display: 'grid', gap: 2 }}>
              <TextField
                label="Activation Token"
                type="text"
                value={token || ''}
                fullWidth
                disabled
                size="small"
                helperText="Token from your activation email"
              />

              <TextField
                label="Password"
                type={showPassword ? 'text' : 'password'}
                value={form.password}
                onChange={(event) => updateField('password', event.target.value)}
                fullWidth
                required
                disabled={loading}
                placeholder="Enter a strong password"
                slotProps={{
                  input: {
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          aria-label={showPassword ? 'Hide password' : 'Show password'}
                          onClick={() => setShowPassword((current) => !current)}
                          onMouseDown={(event) => event.preventDefault()}
                          edge="end"
                          disabled={loading}
                        >
                          {showPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                        </IconButton>
                      </InputAdornment>
                    ),
                  },
                }}
              />

              <TextField
                label="Confirm Password"
                type={showConfirmPassword ? 'text' : 'password'}
                value={form.confirmPassword}
                onChange={(event) => updateField('confirmPassword', event.target.value)}
                fullWidth
                required
                disabled={loading}
                placeholder="Confirm your password"
                slotProps={{
                  input: {
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          aria-label={showConfirmPassword ? 'Hide password' : 'Show password'}
                          onClick={() => setShowConfirmPassword((current) => !current)}
                          onMouseDown={(event) => event.preventDefault()}
                          edge="end"
                          disabled={loading}
                        >
                          {showConfirmPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                        </IconButton>
                      </InputAdornment>
                    ),
                  },
                }}
              />

              <Button
                type="submit"
                variant="contained"
                size="large"
                disabled={loading || !token}
                startIcon={loading ? <CircularProgress size={20} color="inherit" /> : undefined}
              >
                {loading ? 'Activating…' : 'Activate Account'}
              </Button>
            </Box>

            <Divider sx={{ my: 3 }} />

            <Typography variant="body2" color="text.secondary">
              After activation, you'll be redirected to login with your new credentials.
            </Typography>
          </CardContent>
        </Card>
      </Container>
    </Box>
  )
}

export default ActivateClient
