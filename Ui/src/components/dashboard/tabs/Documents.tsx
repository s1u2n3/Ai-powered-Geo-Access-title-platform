import { useState } from 'react'
import { Alert, Box, Button, Card, CardContent, CircularProgress, TextField, Typography } from '@mui/material'
import { processDocumentChunks } from '../../../api/document'

function Documents() {
  const [parcelId, setParcelId] = useState('')
  const [error, setError] = useState('')
  const [result, setResult] = useState<{ jobId: string; message: string } | null>(null)
  const [loading, setLoading] = useState(false)

  const handleProcess = async () => {
    const trimmed = parcelId.trim()
    setError('')
    setResult(null)

    if (!trimmed) {
      setError('Please enter a Parcel ID to continue.')
      return
    }

    setLoading(true)
    try {
      const response = await processDocumentChunks(trimmed)
      setResult({ jobId: response.JobId, message: response.Message })
    } catch (err) {
      setError('Failed to start document chunk processing. Please try again.')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box sx={{ display: 'grid', gap: 3 }}>
      <Box>
        <Typography component="h1" variant="h5" sx={{ fontWeight: 800, color: '#0f172a' }}>
          Document Chunk Request
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          Enter the Parcel ID and start background document chunk processing.
        </Typography>
      </Box>

      <Card sx={{ borderRadius: 2, border: '1px solid rgba(15,23,42,0.08)', boxShadow: '0 18px 45px rgba(15,23,42,0.06)' }}>
        <CardContent sx={{ p: 3, display: 'grid', gap: 2 }}>
          <TextField
            label="Parcel ID"
            size="small"
            fullWidth
            value={parcelId}
            onChange={(event) => setParcelId(event.target.value)}
            placeholder="Enter Parcel ID"
          />

          {error && <Alert severity="error">{error}</Alert>}
          {result && <Alert severity="success">{result.message} (Job ID: {result.jobId})</Alert>}

          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, pt: 1 }}>
            <Button
              variant="contained"
              onClick={handleProcess}
              disabled={loading}
            >
              {loading ? <CircularProgress size={20} color="inherit" /> : 'Start Chunk Processing'}
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  )
}

export default Documents
