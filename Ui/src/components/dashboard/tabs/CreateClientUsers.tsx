import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  CircularProgress,
  Divider,
  FormControl,
  FormControlLabel,
  InputAdornment,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import BusinessIcon from '@mui/icons-material/Business'
import CheckIcon from '@mui/icons-material/Check'
import EmailOutlinedIcon from '@mui/icons-material/EmailOutlined'
import PersonIcon from '@mui/icons-material/Person'
import PhoneIcon from '@mui/icons-material/Phone'
import SecurityOutlinedIcon from '@mui/icons-material/SecurityOutlined'
import WorkspacesOutlinedIcon from '@mui/icons-material/WorkspacesOutlined'
import { createClient, type CreateClientPayload, GetAllCompany, GetBrancheesByCompanyName, type GetBrancheesByCompanyName as BranchType } from '../../../api/client'


const emptyForm = {
  clientCode: '',
  clientName: '',
  clientType: '',
  email: '',
  phoneNumber: '',
  companyName: '',
  branchCode: '',
  canLoadStarter: true,
  canReviewTitle: true,
  canIssueInsurance: true,
}

function CreateClientUsers() {
  const [form, setForm] = useState(emptyForm)
  const [loading, setLoading] = useState(false)
  const [companiesLoading, setCompaniesLoading] = useState(false)
  const [branchesLoading, setBranchesLoading] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [companies, setcompanies] = useState<GetAllCompany[]>([])
  const [branches, setBranches] = useState<BranchType[]>([])

  const payload = useMemo<CreateClientPayload>(
    () => ({
      clientCode: form.clientCode.trim(),
      clientName: form.clientName.trim(),
      clientType: form.clientType.trim(),
      email: form.email.trim(),
      phoneNumber: form.phoneNumber.trim(),
      companies: [
        {
          companyName: form.companyName,
          branches: [
            {
              branchCode: form.branchCode,
              canLoadStarter: form.canLoadStarter,
              canReviewTitle: form.canReviewTitle,
              canIssueInsurance: form.canIssueInsurance,
            },
          ],
        },
      ],
    }),
    [form],
  )

  const updateField = (field: keyof typeof emptyForm, value: string | boolean) => {
    setForm((current) => ({ ...current, [field]: value }))
  }

  const clearForm = () => {
    setForm(emptyForm)
    setMessage('')
    setError('')
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage('')
    setError('')

    if (!payload.clientCode || !payload.clientName || !payload.clientType || !payload.email || !payload.phoneNumber) {
      setError('Please fill client code, client name, client type, email, and phone number.')
      return
    }

    setLoading(true)
    try {
      await createClient(payload)
      setMessage('Client created successfully.')
      setForm(emptyForm)
    } catch (submitError) {
      const submitMessage =
        submitError instanceof Error ? submitError.message : 'Unexpected error while creating client.'
      setError(submitMessage)
    } finally {
      setLoading(false)
    }
  }

  const getAllCompanies = async () => {
    setCompaniesLoading(true)
    setError('')

    try {
      const companies = await GetAllCompany()
      setcompanies(companies)
    } catch (submitError) {
      const submitMessage =
        submitError instanceof Error ? submitError.message : 'Unexpected error while loading companies.'
      setError(submitMessage)
    } finally {
      setCompaniesLoading(false)
    }
  }

  const handleCompanyChange = async (event: any) => {
    const name = event.target.value as string
    updateField('companyName', name)

    if (!name) {
      setBranches([])
      updateField('branchCode', '')
      return
    }

    setBranchesLoading(true)
    setError('')
    try {
      const branchList = await GetBrancheesByCompanyName(name)
      setBranches(branchList || [])
      // clear branch selection if current branch not in new list
      setForm((cur) => ({ ...cur, branchCode: branchList && branchList.length > 0 ? cur.branchCode : '' }))
    } catch (e) {
      const submitMessage = e instanceof Error ? e.message : 'Unexpected error while loading branches.'
      setError(submitMessage)
      setBranches([])
    } finally {
      setBranchesLoading(false)
    }
  }

  useEffect(() => {
    void getAllCompanies()
  }, [])

  return (
    <Box sx={{ minHeight: 'calc(100vh - 58px)', p: 3, bgcolor: '#f7f8ff' }}>
      <Box
        sx={{
          position: 'relative',
          overflow: 'hidden',
          p: { xs: 2.5, md: 3 },
          mb: 2,
          borderRadius: 2,
          background: 'linear-gradient(105deg, #ffffff 0%, #f9f7ff 58%, #eeeaff 100%)',
          border: '1px solid rgba(99, 91, 255, 0.1)',
          boxShadow: '0 18px 60px rgba(99, 91, 255, 0.12)',
        }}
      >
        <Box sx={{ position: 'absolute', right: 44, top: 26, display: { xs: 'none', md: 'block' } }}>
          <Box sx={{ width: 76, height: 76, borderRadius: '50%', bgcolor: '#7c3aed', opacity: 0.9 }} />
          <Box
            sx={{
              position: 'absolute',
              left: 42,
              top: 42,
              width: 34,
              height: 34,
              borderRadius: '50%',
              bgcolor: '#635bff',
              border: '4px solid #ffffff',
              display: 'grid',
              placeItems: 'center',
              color: '#ffffff',
            }}
          >
            <AddIcon />
          </Box>
        </Box>
        <Typography variant="h5" sx={{ fontWeight: 900, color: '#111827', mb: 0.75 }}>
          Create Client
        </Typography>
        <Typography variant="body2" sx={{ color: '#475569', maxWidth: 720 }}>
          Add a client profile and submit the client access flags to the configured API endpoint.
        </Typography>
      </Box>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', lg: '1fr 300px' }, gap: 2 }}>
        <Card sx={{ borderRadius: 2, border: '1px solid rgba(15,23,42,0.08)', boxShadow: '0 18px 55px rgba(15,23,42,0.08)' }}>
          <CardContent sx={{ p: 2.5 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, alignItems: { xs: 'flex-start', md: 'center' }, flexDirection: { xs: 'column', md: 'row' }, mb: 2 }}>
              <Box>
                <Typography variant="h6" sx={{ fontWeight: 900, color: '#111827' }}>
                  Client Details
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Endpoint: POST /api/Client
                </Typography>
              </Box>
              <Chip label="POST /api/Client" color="success" sx={{ fontWeight: 900 }} />
            </Box>

            {(message || error) && (
              <Alert severity={message ? 'success' : 'error'} sx={{ mb: 2 }}>
                {message || error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleSubmit} sx={{ display: 'grid', gap: 2 }}>
              <Box sx={{ border: '1px solid rgba(15,23,42,0.08)', borderRadius: 2, p: 2 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 900, color: '#111827', mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <PersonIcon sx={{ color: '#635bff', fontSize: 18 }} />
                  Client
                </Typography>
                <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 1.5 }}>
                  <TextField
                    label="Client Code"
                    value={form.clientCode}
                    onChange={(event) => updateField('clientCode', event.target.value)}
                    placeholder="Enter client code"
                    size="small"
                    fullWidth
                  />
                  <TextField
                    label="Client Name"
                    value={form.clientName}
                    onChange={(event) => updateField('clientName', event.target.value)}
                    placeholder="Enter client name"
                    size="small"
                    fullWidth
                    slotProps={{ input: { startAdornment: <InputAdornment position="start"><PersonIcon fontSize="small" /></InputAdornment> } }}
                  />
                  <TextField
                    label="Client Type"
                    value={form.clientType}
                    onChange={(event) => updateField('clientType', event.target.value)}
                    placeholder="Enter client type"
                    size="small"
                    fullWidth
                  />
                  <TextField
                    label="Email"
                    value={form.email}
                    onChange={(event) => updateField('email', event.target.value)}
                    placeholder="Enter email address"
                    size="small"
                    type="email"
                    fullWidth
                    slotProps={{ input: { startAdornment: <InputAdornment position="start"><EmailOutlinedIcon fontSize="small" /></InputAdornment> } }}
                  />
                  <TextField
                    label="Phone Number"
                    value={form.phoneNumber}
                    onChange={(event) => updateField('phoneNumber', event.target.value)}
                    placeholder="Enter phone number"
                    size="small"
                    fullWidth
                    slotProps={{ input: { startAdornment: <InputAdornment position="start"><PhoneIcon fontSize="small" /></InputAdornment> } }}
                  />
                </Box>
              </Box>

              <Box sx={{ border: '1px solid rgba(15,23,42,0.08)', borderRadius: 2, p: 2 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 900, color: '#111827', mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <BusinessIcon sx={{ color: '#635bff', fontSize: 18 }} />
                  Company & Branch
                </Typography>
                <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 1.5 }}>
                  <FormControl size="small" fullWidth>
                    <InputLabel id="company-name-label">Company</InputLabel>
                    <Select
                      labelId="company-name-label"
                      label="Company"
                      value={form.companyName}
                      onChange={handleCompanyChange}
                      startAdornment={<InputAdornment position="start"><BusinessIcon fontSize="small" /></InputAdornment>}
                    >
                      <MenuItem value="" disabled>
                        {companiesLoading ? 'Loading companies...' : 'Select company'}
                      </MenuItem>
                      {companies.map((company) => (
                        <MenuItem key={company.id} value={company.name}>
                          {company.name}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                  <FormControl size="small" fullWidth>
                    <InputLabel id="branch-code-label">Branch</InputLabel>
                    <Select
                      labelId="branch-code-label"
                      label="Branch"
                      value={form.branchCode}
                      onChange={(event) => updateField('branchCode', event.target.value)}
                      startAdornment={<InputAdornment position="start"><WorkspacesOutlinedIcon fontSize="small" /></InputAdornment>}
                    >
                      <MenuItem value="" disabled>
                        {branchesLoading ? 'Loading branches...' : 'Select branch'}
                      </MenuItem>
                      {branches.map((b) => (
                        <MenuItem key={b.id} value={b.branchCode}>
                          {b.branchCode}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Box>
              </Box>

              <Box sx={{ border: '1px solid rgba(15,23,42,0.08)', borderRadius: 2, p: 2 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 900, color: '#111827', mb: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <SecurityOutlinedIcon sx={{ color: '#635bff', fontSize: 18 }} />
                  Branch Access
                </Typography>
                <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(3, 1fr)' }, gap: 1 }}>
                  <FormControlLabel
                    control={<Checkbox checked={form.canLoadStarter} onChange={(event) => updateField('canLoadStarter', event.target.checked)} />}
                    label="Can Load Starter"
                  />
                  <FormControlLabel
                    control={<Checkbox checked={form.canReviewTitle} onChange={(event) => updateField('canReviewTitle', event.target.checked)} />}
                    label="Can Review Title"
                  />
                  <FormControlLabel
                    control={<Checkbox checked={form.canIssueInsurance} onChange={(event) => updateField('canIssueInsurance', event.target.checked)} />}
                    label="Can Issue Insurance"
                  />
                </Box>
              </Box>

              <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 1.5, flexWrap: 'wrap' }}>
                <Button type="button" variant="outlined" color="inherit" onClick={clearForm} disabled={loading} sx={{ minWidth: 120, fontWeight: 800 }}>
                  Clear Form
                </Button>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={loading}
                  startIcon={loading ? <CircularProgress size={16} color="inherit" /> : <CheckIcon />}
                  sx={{ minWidth: 140, fontWeight: 900, boxShadow: '0 10px 22px rgba(99,91,255,0.28)' }}
                >
                  Create Client
                </Button>
              </Box>
            </Box>
          </CardContent>
        </Card>

        <Card sx={{ borderRadius: 2, border: '1px solid rgba(15,23,42,0.08)', boxShadow: '0 18px 55px rgba(15,23,42,0.08)', alignSelf: 'start' }}>
          <CardContent sx={{ p: 2 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 900, display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
              <SecurityOutlinedIcon sx={{ color: '#635bff', fontSize: 18 }} />
              Request Payload
            </Typography>
            <Divider sx={{ mb: 1.5 }} />
            <Box
              component="pre"
              sx={{
                m: 0,
                p: 1.5,
                bgcolor: '#0f172a',
                color: '#e2e8f0',
                borderRadius: 1,
                fontSize: 12,
                lineHeight: 1.6,
                overflow: 'auto',
                whiteSpace: 'pre-wrap',
                wordBreak: 'break-word',
              }}
            >
              {JSON.stringify(payload, null, 2)}
            </Box>
          </CardContent>
        </Card>
      </Box>
    </Box>
  )
}

export default CreateClientUsers
