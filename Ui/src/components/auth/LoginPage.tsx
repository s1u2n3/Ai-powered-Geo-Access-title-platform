import LockOutlinedIcon from '@mui/icons-material/LockOutlined'
import type { FormEvent } from 'react'
import {
  Avatar,
  Box,
  Button,
  Card,
  CardContent,
  Container,
  Divider,
  TextField,
  Typography,
} from '@mui/material'

interface LoginPageProps {
  email: string
  password: string
  message: string
  loading: boolean
  onEmailChange: (value: string) => void
  onPasswordChange: (value: string) => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
}

function LoginPage({
  email,
  password,
  message,
  loading,
  onEmailChange,
  onPasswordChange,
  onSubmit,
}: LoginPageProps) {
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
              Secure Client Access
            </Typography>
            <Typography variant="body1" sx={{ color: 'rgba(248, 250, 252, 0.84)', maxWidth: 520 }}>
              Sign in to access your property search, documents, geo controls, and profile tools in a professional portal.
            </Typography>
          </Box>

          <CardContent sx={{ p: 5, background: '#f8fafc' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
              <Avatar sx={{ bgcolor: '#4f46e5' }}>
                <LockOutlinedIcon />
              </Avatar>
              <div>
                <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }}>
                  Login to your dashboard
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Enter your credentials to continue.
                </Typography>
              </div>
            </Box>

            <Box component="form" onSubmit={onSubmit} noValidate sx={{ display: 'grid', gap: 2 }}>
              <TextField
                label="Email address"
                type="email"
                value={email}
                onChange={(event) => onEmailChange(event.target.value)}
                fullWidth
                required
                disabled={loading}
              />
              <TextField
                label="Password"
                type="password"
                value={password}
                onChange={(event) => onPasswordChange(event.target.value)}
                fullWidth
                required
                disabled={loading}
              />
              <Button type="submit" variant="contained" size="large" disabled={loading}>
                {loading ? 'Signing in…' : 'Continue'}
              </Button>
            </Box>

            <Divider sx={{ my: 3 }} />

            <Typography variant="body2" color="text.secondary">
              {message || 'Use your client credentials to unlock the secure access portal.'}
            </Typography>
          </CardContent>
        </Card>
      </Container>
    </Box>
  )
}

export default LoginPage
