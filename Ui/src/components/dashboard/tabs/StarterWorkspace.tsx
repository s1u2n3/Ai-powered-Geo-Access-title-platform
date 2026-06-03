import { useState } from 'react'
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Divider,
  Grid,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  Alert,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import WarningIcon from '@mui/icons-material/Warning'
import ErrorIcon from '@mui/icons-material/Error'
import HomeIcon from '@mui/icons-material/Home'
import DescriptionIcon from '@mui/icons-material/Description'
import GavelIcon from '@mui/icons-material/Gavel'
import AccountBalanceIcon from '@mui/icons-material/AccountBalance'
import SecurityIcon from '@mui/icons-material/Security'

interface StarterWorkspaceProps {
  starterJson: string
  parcelId?: string
  county?: string
  state?: string
  onBack: () => void
}

interface StarterData {
  parcelId: string
  propertyAddress: string
  county: string
  state: string
  legalDescription: string
  vestingInformation: {
    currentOwners: string[]
    vestingType: string
    ownershipPercentage: string
  }
  deedInformation: {
    instrumentNumber: string
    bookPage: string
    recordingDate: string
    effectiveDate: string
    grantors: string[]
    grantees: string[]
  }
  taxInformation: {
    parcelNumber: string
    taxStatus: string
    delinquentTaxes: boolean
    taxAmountDue: string
  }
  mortgages: Array<{
    instrumentNumber: string
    recordingDate: string
    lender: string
    borrower: string
    amount: string
    status: string
  }>
  liens: Array<any>
  judgments: Array<{
    courtCaseRef: string
    status: string
  }>
  easements: Array<any>
  restrictions: Array<any>
  encumbrances: Array<{
    instrumentNumber: string
    recordingDate: string
    type: string
    status: string
  }>
  chainOfTitle: Array<any>
  requirements: Array<any>
  exceptions: Array<any>
  titleIssues: Array<{
    issueType: string
    severity: string
    description: string
    recommendedAction: string
  }>
  missingDocuments: Array<any>
  documentsReviewed: Array<any>
  riskAssessment: {
    overallRisk: string
    riskFactors: Array<{
      factor: string
      severity: string
    }>
  }
  titleSummary: string
  confidenceScore: number
}

function StarterWorkspace({ starterJson, parcelId, county, state, onBack }: StarterWorkspaceProps) {
  const [selectedIssue, setSelectedIssue] = useState<any>(null)
  const [selectedFact, setSelectedFact] = useState<any>(null)
  
  let data: any
  try {
    data = JSON.parse(starterJson)
  } catch (error) {
    return (
      <Box sx={{ p: 3 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={onBack} sx={{ mb: 2 }}>
          Back
        </Button>
        <Alert severity="error">Invalid Starter JSON data</Alert>
      </Box>
    )
  }

  const getSeverityColor = (severity: string) => {
    switch (severity?.toUpperCase()) {
      case 'HIGH':
        return 'error'
      case 'MEDIUM':
        return 'warning'
      case 'LOW':
        return 'success'
      default:
        return 'default'
    }
  }

  const getRiskColor = (risk: string) => {
    switch (risk?.toUpperCase()) {
      case 'HIGH':
        return '#ef4444'
      case 'MEDIUM':
        return '#f59e0b'
      case 'LOW':
        return '#10b981'
      default:
        return '#6b7280'
    }
  }

  const prettifyKey = (k: string) => {
    return k
      .replace(/([A-Z])/g, ' $1')
      .replace(/[_-]/g, ' ')
      .replace(/\s+/g, ' ')
      .trim()
      .replace(/\b\w/g, (c) => c.toUpperCase())
  }

  const renderCardRow = (label: string, value: any) => (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 1, alignItems: 'center', py: 0.5 }}>
      <Typography variant="caption" sx={{ color: '#6b7280' }}>{label}</Typography>
      <Typography variant="body2" sx={{ fontWeight: 700, textAlign: 'right' }}>{value || '—'}</Typography>
    </Box>
  )

  const formatDocumentRows = (value: any) => {
    if (value === null || value === undefined || value === '') {
      return renderCardRow('Status', 'Empty')
    }

    if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
      return renderCardRow('Value', String(value))
    }

    if (Array.isArray(value)) {
      return value.slice(0, 5).map((item, idx) => (
        <Typography key={idx} variant="caption" sx={{ display: 'block', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }} title={typeof item === 'object' ? JSON.stringify(item) : String(item)}>
          • {typeof item === 'object' ? JSON.stringify(item) : String(item)}
        </Typography>
      ))
    }

    const entries = Object.entries(value)
    const customOrder = ['seller', 'buyer', 'value', 'status', 'registrationRef', 'khataNumber', 'amount', 'created', 'risk', 'cases', 'approval', 'permit']
    const ordered = entries.slice().sort(([a], [b]) => {
      const aIndex = customOrder.indexOf(a)
      const bIndex = customOrder.indexOf(b)
      if (aIndex !== -1 || bIndex !== -1) {
        if (aIndex === -1) return 1
        if (bIndex === -1) return -1
        return aIndex - bIndex
      }
      return a.localeCompare(b)
    })

    return ordered.slice(0, 6).map(([k, v], idx) => (
      <Box key={idx} sx={{ display: 'flex', justifyContent: 'space-between', gap: 1, alignItems: 'center', py: 0.35 }}>
        <Typography variant="caption" sx={{ color: '#6b7280' }}>{prettifyKey(k)}</Typography>
        <Typography variant="caption" sx={{ fontWeight: 700, textAlign: 'right' }} title={typeof v === 'object' ? JSON.stringify(v) : String(v)}>
          {typeof v === 'object' ? JSON.stringify(v) : String(v) || '—'}
        </Typography>
      </Box>
    ))
  }

  const primaryStarterDocs = [
    'saleDeed',
    'mortgageDeed',
    'encumbranceCertificate',
    'courtCaseRecord',
    'khataExtract',
    'ownerKyc',
  ]

  const renderDocumentCard = (key: string, val: any) => (
    <Card
      onClick={() => setSelectedFact({ key, value: val })}
      sx={{
        bgcolor: '#ffffff',
        border: '1px solid rgba(15,23,42,0.06)',
        cursor: 'pointer',
        display: 'flex',
        flexDirection: 'column',
        width: '100%',
        height: '100%',
        '&:hover': { boxShadow: '0 8px 20px rgba(0,0,0,0.08)' },
      }}
    >
      <CardContent sx={{ flex: 1, overflow: 'hidden', minHeight: 170, display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
        <Box>
          <Typography variant="subtitle2" sx={{ fontWeight: 800, mb: 1 }}>{prettifyKey(key)}</Typography>
          {formatDocumentRows(val)}
        </Box>
      </CardContent>
    </Card>
  )

  const orderedStarterDocs = (() => {
    const entries = Object.entries(data.starterFacts || {})
    const primary = entries.filter(([key]) => primaryStarterDocs.includes(key))
    const secondary = entries.filter(([key]) => !primaryStarterDocs.includes(key)).sort(([a], [b]) => a.localeCompare(b))
    const orderedPrimary = primaryStarterDocs.map((key) => primary.find(([k]) => k === key)).filter(Boolean) as Array<[string, any]>
    return [...orderedPrimary, ...secondary]
  })()

  const verificationItems = [
    { label: 'Owner KYC', value: data.starterFacts?.ownerKyc ? 'Verified' : 'Pending' },
    { label: 'Address Verified', value: data.starterFacts?.khataExtract ? 'Yes' : 'No' },
    { label: 'Photo Match', value: data.vestingInformation?.currentOwners?.length ? 'Yes' : 'No' },
    { label: 'Status', value: data.taxInformation?.taxStatus === 'PAID' ? 'Complete' : 'Pending' },
  ]

  const summaryItems = [
    { label: 'Parcel ID', value: data.parcelId || parcelId || '—', icon: <DescriptionIcon sx={{ color: '#4338ca' }} /> },
    { label: 'County / State', value: `${data.county || county || '—'} / ${data.state || state || '—'}`, icon: <HomeIcon sx={{ color: '#4338ca' }} /> },
    { label: 'Address', value: data.propertyAddress || 'Not available', icon: <HomeIcon sx={{ color: '#4338ca' }} /> },
    { label: 'Overall Risk', value: data.riskAssessment?.overallRisk || 'Unknown', icon: <SecurityIcon sx={{ color: '#16a34a' }} />, highlight: true },
  ]

  const renderValue = (value: any) => {
    if (value === null || value === undefined || value === '') {
      return <Typography variant="body2" sx={{ color: '#6b7280' }}>—</Typography>
    }

    if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
      return (
        <Typography variant="body2" sx={{ fontWeight: 600, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }} title={String(value)}>
          {String(value)}
        </Typography>
      )
    }

    if (Array.isArray(value)) {
      return (
        <Box>
          {value.slice(0, 5).map((v, i) => (
            <Typography key={i} variant="caption" sx={{ display: 'block', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }} title={typeof v === 'object' ? JSON.stringify(v) : String(v)}>
              • {typeof v === 'object' ? JSON.stringify(v) : String(v)}
            </Typography>
          ))}
          {value.length > 5 && <Typography variant="caption" sx={{ color: '#6b7280' }}>and {value.length - 5} more…</Typography>}
        </Box>
      )
    }

    if (typeof value === 'object') {
      const entries = Object.entries(value)
      const maxShow = 8
      return (
        <Box sx={{ display: 'grid', gap: 0.5 }}>
          {entries.slice(0, maxShow).map(([k, v], i) => (
            <Box key={i} sx={{ display: 'grid', gridTemplateColumns: '1fr auto', gap: 1, alignItems: 'center' }}>
              <Typography variant="caption" sx={{ fontWeight: 700, color: '#374151' }}>{prettifyKey(k)}</Typography>
              <Typography variant="caption" sx={{ color: '#111827', textAlign: 'right', maxWidth: '60%' }} title={typeof v === 'object' ? JSON.stringify(v) : String(v)}>
                {typeof v === 'object' ? JSON.stringify(v) : String(v) || '—'}
              </Typography>
            </Box>
          ))}
          {entries.length > maxShow && (
            <Typography variant="caption" sx={{ color: '#6b7280' }}>and {entries.length - maxShow} more…</Typography>
          )}
        </Box>
      )
    }

    return <Typography variant="body2">{String(value)}</Typography>
  }

  return (
    <Box sx={{ p: 3, bgcolor: '#f8fafc', minHeight: '100vh' }}>
      <Button startIcon={<ArrowBackIcon />} onClick={onBack} sx={{ mb: 3 }}>
        Back to Property Search
      </Button>

      <Typography variant="h4" sx={{ fontWeight: 800, mb: 2 }}>
        Starter Workspace - {data.parcelId || parcelId || 'Unknown Parcel'}
      </Typography>
      <Typography variant="subtitle1" sx={{ color: '#475569', mb: 4 }}>
        Overview of key property information and documents.
      </Typography>

      <Grid container spacing={2} sx={{ mb: 3 }}>
        {summaryItems.map((item) => (
          <Grid item xs={12} sm={6} md={3} key={item.label}>
            <Card sx={{ bgcolor: item.highlight ? '#ecfdf5' : '#ffffff', border: '1px solid rgba(15,23,42,0.08)', minHeight: 120 }}>
              <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', width: 42, height: 42, borderRadius: 1, bgcolor: item.highlight ? '#dcfce7' : '#eef2ff' }}>
                  {item.icon}
                </Box>
                <Box>
                  <Typography variant="caption" sx={{ color: '#64748b', display: 'block', mb: 0.5 }}>
                    {item.label}
                  </Typography>
                  <Typography variant="h6" sx={{ fontWeight: 700, color: item.highlight ? '#166534' : '#0f172a' }}>
                    {item.value}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={2} sx={{ mb: 4 }}>
        <Grid item xs={12} md={4}>
          <Card sx={{ border: '1px solid rgba(15,23,42,0.08)', minHeight: 220 }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontWeight: 800, mb: 1 }}>Property Information</Typography>
              <Divider sx={{ mb: 2 }} />
              {renderCardRow('Current Owners', Array.isArray(data.vestingInformation?.currentOwners) ? data.vestingInformation.currentOwners.join(', ') : '—')}
              {renderCardRow('Vesting Type', data.vestingInformation?.vestingType || 'Not specified')}
              {renderCardRow('Ownership %', data.vestingInformation?.ownershipPercentage || 'Not specified')}
              {renderCardRow('Tax Status', data.taxInformation?.taxStatus || '—')}
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={4}>
          <Card sx={{ border: '1px solid rgba(15,23,42,0.08)', minHeight: 220 }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontWeight: 800, mb: 1 }}>Risk Assessment</Typography>
              <Divider sx={{ mb: 2 }} />
              {renderCardRow('Overall Risk', data.riskAssessment?.overallRisk || 'Unknown')}
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mt: 1 }}>
                {(Array.isArray(data.riskAssessment?.riskFactors) ? data.riskAssessment.riskFactors : []).slice(0, 6).map((factor: any, idx: number) => (
                  <Chip
                    key={idx}
                    label={typeof factor === 'string' ? factor : factor.factor || String(factor)}
                    size="small"
                    color="warning"
                    sx={{ fontWeight: 700 }}
                  />
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={4}>
          <Card sx={{ border: '1px solid rgba(15,23,42,0.08)', minHeight: 220 }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontWeight: 800, mb: 1 }}>Key Verification</Typography>
              <Divider sx={{ mb: 2 }} />
              {verificationItems.map((item) => (
                <Box key={item.label} sx={{ display: 'flex', justifyContent: 'space-between', gap: 1, alignItems: 'center', py: 0.75 }}>
                  <Typography variant="body2" sx={{ color: '#475569' }}>{item.label}</Typography>
                  <Typography variant="body2" sx={{ fontWeight: 700 }}>{item.value}</Typography>
                </Box>
              ))}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Title Issues */}
      {data.titleIssues && data.titleIssues.length > 0 && (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h6" sx={{ fontWeight: 800, mb: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
            <ErrorIcon sx={{ color: '#dc2626' }} />
            Title Issues ({data.titleIssues.length})
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            {data.titleIssues.map((issue: any, idx: number) => {
              const isPrimitive = typeof issue === 'string' || typeof issue === 'number'
              const title = isPrimitive ? String(issue) : issue.issueType || JSON.stringify(issue)
              const severity = isPrimitive ? '' : (issue.severity || '')
              return (
                <Grid item xs={12} sm={6} md={4} key={idx}>
                  <Card
                    sx={{
                      bgcolor: '#ffffff',
                      border: '1px solid rgba(15,23,42,0.06)',
                      cursor: 'pointer',
                      display: 'flex',
                      flexDirection: 'column',
                      height: '100%',
                      '&:hover': { boxShadow: '0 10px 25px rgba(0,0,0,0.1)' },
                    }}
                    onClick={() => setSelectedIssue(issue)}
                  >
                    <CardContent sx={{ flex: 1, display: 'flex', flexDirection: 'column', justifyContent: 'space-between', minHeight: 120 }}>
                      <Box>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                          <Typography variant="body2" sx={{ fontWeight: 700 }}>{title}</Typography>
                          <Chip label={severity || 'N/A'} size="small" color={getSeverityColor(severity)} />
                        </Box>
                        {!isPrimitive && (
                          <Typography variant="caption" sx={{ color: '#6b7280', display: 'block', mb: 1 }}>{issue.description}</Typography>
                        )}
                      </Box>
                      <Typography variant="caption" sx={{ color: '#3b82f6', fontWeight: 600 }}>Click for details →</Typography>
                    </CardContent>
                  </Card>
                </Grid>
              )
            })}
          </Grid>
        </Box>
      )}

      {/* Documents & Records */}
      {data.starterFacts && Object.keys(data.starterFacts).length > 0 && (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h6" sx={{ fontWeight: 800, mb: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
            <DescriptionIcon sx={{ color: '#4338ca' }} />
            Documents & Records
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={2} alignItems="stretch">
            {orderedStarterDocs.map(([key, val]) => (
              <Grid item xs={12} sm={6} md={4} key={key} sx={{ display: 'flex' }}>
                {renderDocumentCard(key, val)}
              </Grid>
            ))}
          </Grid>
        </Box>
      )}

      {/* Starter Fact Details Dialog */}
      <Dialog open={!!selectedFact} onClose={() => setSelectedFact(null)} maxWidth="md" fullWidth>
        <DialogTitle sx={{ fontWeight: 700 }}>{selectedFact?.key}</DialogTitle>
        <DialogContent dividers>
          <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1 }}>Value</Typography>
          <Box sx={{ bgcolor: '#f3f4f6', p: 2, borderRadius: 1, fontFamily: 'monospace', whiteSpace: 'pre-wrap' }}>
            {selectedFact ? JSON.stringify(selectedFact.value, null, 2) : ''}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSelectedFact(null)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Issue Details Dialog */}
      <Dialog open={!!selectedIssue} onClose={() => setSelectedIssue(null)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 700 }}>
          {typeof selectedIssue === 'string' ? selectedIssue : (selectedIssue?.issueType || JSON.stringify(selectedIssue))}
        </DialogTitle>
        <DialogContent dividers>
          {typeof selectedIssue === 'string' ? (
            <Box>
              <Typography variant="body2">{selectedIssue}</Typography>
            </Box>
          ) : (
            <>
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1 }}>
                  Description
                </Typography>
                <Typography variant="body2">{selectedIssue?.description || JSON.stringify(selectedIssue)}</Typography>
              </Box>

              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1 }}>
                  Severity
                </Typography>
                <Chip
                  label={selectedIssue?.severity || 'N/A'}
                  color={selectedIssue?.severity === 'HIGH' ? 'error' : selectedIssue?.severity === 'MEDIUM' ? 'warning' : 'default'}
                />
              </Box>

              <Box>
                <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1 }}>
                  Recommended Action
                </Typography>
                <Typography variant="body2" sx={{ bgcolor: '#eff6ff', p: 2, borderRadius: 1, color: '#1e40af' }}>
                  {selectedIssue?.recommendedAction || '—'}
                </Typography>
              </Box>
            </>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSelectedIssue(null)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default StarterWorkspace
