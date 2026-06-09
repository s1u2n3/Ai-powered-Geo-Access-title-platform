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

const starterStorageKey = 'adminStarterJson'

const sampleStarterJson = `{
  "parcelSummary": {
    "surveyNo": "117/3B",
    "hissaNo": "3B",
    "village": "Hebbal Extension",
    "hobli": "Kasaba Hobli",
    "taluk": "Mysuru",
    "district": "Mysuru",
    "state": "Karnataka",
    "totalArea": 1200,
    "shapeOfPlot": "Rectangular"
  },
  "ownership": {
    "ownerName": "Ravi Kumar",
    "propertyAddress": "No. 12, 3rd Cross, Hebbal Extension, Mysuru - 570017",
    "propertyType": "Residential - Self-Occupied"
  },
  "encumbrances": [
    {
      "ecNo": "EC-MYS-2024-14421",
      "formType": "Form 15 (Encumbrances Exist) - Mortgage Released",
      "periodCovered": "01 April 2015 to 31 March 2024 (9 years)",
      "issuedBy": "Sub-Registrar, Mysuru",
      "dateOfIssue": "5 April 2024",
      "appliedBy": "Ravi Kumar",
      "property": "Parcel ID P-1001, Survey No. 189/3B, Hebbal Extension, Mysuru"
    }
  ]
}`

function LoadFullStarter() {
  const [starterJson, setStarterJson] = useState(() => window.localStorage.getItem(starterStorageKey) || sampleStarterJson)
  const [message, setMessage] = useState('')
  const [messageType, setMessageType] = useState<'success' | 'error'>('success')

  const handleLoad = () => {
    try {
      JSON.parse(starterJson)
      window.localStorage.setItem(starterStorageKey, starterJson)
      setMessageType('success')
      setMessage('Starter JSON saved. The API call can be connected here.')
    } catch {
      setMessageType('error')
      setMessage('Invalid JSON. Please fix the starter data before loading it.')
    }
  }

  return (
    <Box sx={{ display: 'grid', gap: 3 }}>
      <Box>
        <Typography component="h1" variant="h5" sx={{ fontWeight: 800, color: '#0f172a' }}>
          Load Full Starter
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          Paste the complete starter JSON and keep it ready for the load API.
        </Typography>
      </Box>

      <Card sx={{ borderRadius: 2, border: '1px solid rgba(15,23,42,0.08)', boxShadow: '0 18px 45px rgba(15,23,42,0.06)' }}>
        <CardContent sx={{ p: 3 }}>
          {message && <Alert severity={messageType} sx={{ mb: 2 }}>{message}</Alert>}
          <TextField
            label="Starter JSON"
            value={starterJson}
            onChange={(event) => setStarterJson(event.target.value)}
            minRows={18}
            multiline
            fullWidth
            sx={{
              '& textarea': {
                fontFamily: 'Consolas, monospace',
                fontSize: '0.86rem',
                lineHeight: 1.55,
              },
            }}
          />

          <Box sx={{ mt: 3, display: 'flex', justifyContent: 'flex-end' }}>
            <Button variant="contained" startIcon={<PlayCircleIcon />} onClick={handleLoad}>
              Load Starter
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  )
}

export default LoadFullStarter
