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
  Typography,
  Alert,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import ErrorIcon from '@mui/icons-material/Error'
import HomeIcon from '@mui/icons-material/Home'
import DescriptionIcon from '@mui/icons-material/Description'
import SecurityIcon from '@mui/icons-material/Security'

interface StarterWorkspaceProps {
  starterJson?: string
  parcelId?: string
  county?: string
  state?: string
  onBack?: () => void
}

function StarterWorkspace({ starterJson = '{}', parcelId, county, state, onBack = () => {} }: StarterWorkspaceProps) {
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

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: 'repeat(4, 1fr)' }, gap: 2, mb: 3 }}>
        {summaryItems.map((item) => (
          <Box key={item.label}>
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
          </Box>
        ))}
      </Box>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(3, 1fr)' }, gap: 2, mb: 4 }}>
        <Box>
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
        </Box>

        <Box>
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
        </Box>

        <Box>
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
        </Box>
      </Box>

      {/* Title Issues */}
      {data.titleIssues && data.titleIssues.length > 0 && (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h6" sx={{ fontWeight: 800, mb: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
            <ErrorIcon sx={{ color: '#dc2626' }} />
            Title Issues ({data.titleIssues.length})
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: 'repeat(3, 1fr)' }, gap: 2 }}>
            {data.titleIssues.map((issue: any, idx: number) => {
              const isPrimitive = typeof issue === 'string' || typeof issue === 'number'
              const title = isPrimitive ? String(issue) : issue.issueType || JSON.stringify(issue)
              const severity = isPrimitive ? '' : (issue.severity || '')
              return (
                <Box key={idx}>
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
                </Box>
              )
            })}
          </Box>
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
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: 'repeat(3, 1fr)' }, gap: 2, alignItems: 'stretch' }}>
            {orderedStarterDocs.map(([key, val]) => (
              <Box key={key} sx={{ display: 'flex' }}>
                {renderDocumentCard(key, val)}
              </Box>
            ))}
          </Box>
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
