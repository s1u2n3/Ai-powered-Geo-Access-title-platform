import { useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  TextField,
  Typography,
} from '@mui/material'
import PlayCircleIcon from '@mui/icons-material/PlayCircle'
import { analyzeParcel } from '../../../api/apiClient'

function getErrorMessage(err: any) {
  const data = err?.response?.data

  if (typeof data === 'string') {
    return data
  }

  if (typeof data?.message === 'string') {
    return data.message
  }

  if (typeof data?.Message === 'string') {
    return data.Message
  }

  return 'Parcel not found'
}

function LoadFullStarter() {
  const [parcelId, setParcelId] = useState('')
  const [message, setMessage] = useState('')
  const [messageType, setMessageType] = useState<'success' | 'error'>('success')
  const [loading, setLoading] = useState(false)

  const handleLoad = async () => {
    if (!parcelId) {
      setMessageType('error')
      setMessage('Parcel ID is required')
      return
    }

    setLoading(true)
    setMessage('')
    try {
      const resp = await analyzeParcel(parcelId, 'Load Full Starter')
      setMessageType('success')
      setMessage('Starter loaded successfully')
      console.log('Analyze parcel response', resp.data)
    } catch (err: any) {
      setMessageType('error')
      setMessage(getErrorMessage(err))
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box sx={{ display: 'grid', gap: 3 }}>
      <Box>
        <Typography component="h1" variant="h5" sx={{ fontWeight: 800, color: '#0f172a' }}>
          Load Full Starter
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          Enter a Parcel ID and click Load to generate and persist a full starter package.
        </Typography>
      </Box>

      <Card sx={{ borderRadius: 2, border: '1px solid rgba(15,23,42,0.08)', boxShadow: '0 18px 45px rgba(15,23,42,0.06)' }}>
        <CardContent sx={{ p: 3 }}>
          {message && <Alert severity={messageType} sx={{ mb: 2 }}>{message}</Alert>}

          <TextField
            label="Parcel ID"
            value={parcelId}
            onChange={(event) => setParcelId(event.target.value)}
            fullWidth
          />

          <Box sx={{ mt: 3, display: 'flex', justifyContent: 'flex-end' }}>
            <Button variant="contained" startIcon={<PlayCircleIcon />} onClick={handleLoad} disabled={loading}>
              {loading ? 'Loading...' : 'Load Starter'}
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  )
}

export default LoadFullStarter
