import {
  Box,
  Card,
  CardContent,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Skeleton,
  Typography,
} from '@mui/material'
import { useClient } from '../../../contexts'

function ClientDashBoard() {
  const { clientDetails, selectedCompanyId, selectedBranchId, loadingDetails, setSelectedCompanyId, setSelectedBranchId } =
    useClient()

  const selectedCompany = clientDetails?.companies.find((c) => c.companyId === selectedCompanyId) || null
  const selectedBranch = selectedCompany?.branches.find((b) => b.branchId === selectedBranchId) || null

  if (loadingDetails) {
    return (
      <Box sx={{ display: 'grid', gap: 3 }}>
        <Skeleton variant="rectangular" height={320} />
        <Skeleton variant="rectangular" height={200} />
      </Box>
    )
  }

  if (!clientDetails) {
    return (
      <Box sx={{ textAlign: 'center', py: 6 }}>
        <Typography color="text.secondary">Loading client details...</Typography>
      </Box>
    )
  }

  const handleCompanyChange = (companyId: number) => {
    setSelectedCompanyId(companyId)
    const company = clientDetails.companies.find((c) => c.companyId === companyId)
    if (company && company.branches.length > 0) {
      setSelectedBranchId(company.branches[0].branchId)
    }
  }

  const handleBranchChange = (branchId: number) => {
    setSelectedBranchId(branchId)
  }

  return (
    <Box sx={{ display: 'grid', gap: 3 }}>
      {/* Client Info Card */}
      <Card sx={{ borderRadius: 3, boxShadow: '0 10px 30px rgba(15,23,42,0.08)' }}>
        <CardContent sx={{ p: 4 }}>
          <Typography component="h2" variant="h5" sx={{ fontWeight: 700, mb: 3, color: '#0f172a' }}>
            Client Information
          </Typography>

          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 3 }}>
            <Box>
              <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                Client Name
              </Typography>
              <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
                {clientDetails.clientName}
              </Typography>
            </Box>

            <Box>
              <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                Client Code
              </Typography>
              <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
                {clientDetails.clientCode}
              </Typography>
            </Box>

            <Box>
              <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                Client Type
              </Typography>
              <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
                {clientDetails.clientType}
              </Typography>
            </Box>

            <Box>
              <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                Email
              </Typography>
              <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
                {clientDetails.email}
              </Typography>
            </Box>

            <Box>
              <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                Phone Number
              </Typography>
              <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
                {clientDetails.phoneNumber}
              </Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>

      {/* Company & Branch Selection */}
      <Card sx={{ borderRadius: 3, boxShadow: '0 10px 30px rgba(15,23,42,0.08)' }}>
        <CardContent sx={{ p: 4 }}>
          <Typography component="h2" variant="h5" sx={{ fontWeight: 700, mb: 3, color: '#0f172a' }}>
            Company & Branch Selection
          </Typography>

          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 3 }}>
            <FormControl fullWidth>
              <InputLabel id="company-select-label" sx={{ color: '#64748b' }}>
                Company
              </InputLabel>
              <Select
                labelId="company-select-label"
                id="company-select"
                value={selectedCompanyId || ''}
                label="Company"
                onChange={(e) => handleCompanyChange(e.target.value as number)}
                sx={{
                  '& .MuiOutlinedInput-root': {
                    color: '#0f172a',
                    '& fieldset': {
                      borderColor: 'rgba(15, 23, 42, 0.12)',
                    },
                    '&:hover fieldset': {
                      borderColor: 'rgba(79, 70, 229, 0.3)',
                    },
                  },
                }}
              >
                {clientDetails.companies.map((company) => (
                  <MenuItem key={company.companyId} value={company.companyId}>
                    {company.companyName}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <FormControl fullWidth disabled={!selectedCompanyId}>
              <InputLabel id="branch-select-label" sx={{ color: '#64748b' }}>
                Branch
              </InputLabel>
              <Select
                labelId="branch-select-label"
                id="branch-select"
                value={selectedBranchId || ''}
                label="Branch"
                onChange={(e) => handleBranchChange(e.target.value as number)}
                sx={{
                  '& .MuiOutlinedInput-root': {
                    color: '#0f172a',
                    '& fieldset': {
                      borderColor: 'rgba(15, 23, 42, 0.12)',
                    },
                    '&:hover fieldset': {
                      borderColor: 'rgba(79, 70, 229, 0.3)',
                    },
                  },
                }}
              >
                {selectedCompany?.branches.map((branch) => (
                  <MenuItem key={branch.branchId} value={branch.branchId}>
                    {branch.branchName} ({branch.branchCode})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
        </CardContent>
      </Card>

      {/* Branch Details */}
      {selectedBranch && (
        <Card sx={{ borderRadius: 3, boxShadow: '0 10px 30px rgba(15,23,42,0.08)' }}>
          <CardContent sx={{ p: 4 }}>
            <Typography component="h2" variant="h5" sx={{ fontWeight: 700, mb: 3, color: '#0f172a' }}>
              Selected Branch Details
            </Typography>

            <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 3 }}>
              <Box>
                <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                  Branch Name
                </Typography>
                <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
                  {selectedBranch.branchName}
                </Typography>
              </Box>

              <Box>
                <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                  Branch Code
                </Typography>
                <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
                  {selectedBranch.branchCode}
                </Typography>
              </Box>

              <Box>
                <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                  Branch Type
                </Typography>
                <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
                  {selectedBranch.branchType}
                </Typography>
              </Box>

              <Box>
                <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                  Can Load Starter
                </Typography>
                <Typography variant="body1" sx={{ color: selectedBranch.canLoadStarter ? '#059669' : '#dc2626', fontWeight: 500 }}>
                  {selectedBranch.canLoadStarter ? 'Yes' : 'No'}
                </Typography>
              </Box>

              <Box>
                <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                  Can Review Title
                </Typography>
                <Typography variant="body1" sx={{ color: selectedBranch.canReviewTitle ? '#059669' : '#dc2626', fontWeight: 500 }}>
                  {selectedBranch.canReviewTitle ? 'Yes' : 'No'}
                </Typography>
              </Box>

              <Box>
                <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
                  Can Issue Insurance
                </Typography>
                <Typography variant="body1" sx={{ color: selectedBranch.canIssueInsurance ? '#059669' : '#dc2626', fontWeight: 500 }}>
                  {selectedBranch.canIssueInsurance ? 'Yes' : 'No'}
                </Typography>
              </Box>

              {selectedBranch.geoAccess.length > 0 && (
                <Box sx={{ gridColumn: '1 / -1' }}>
                  <Divider sx={{ my: 1 }} />
                  <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mt: 2, mb: 1 }}>
                    Geographic Access
                  </Typography>
                  {selectedBranch.geoAccess.map((geo) => (
                    <Typography key={geo.stateCode} variant="body2" sx={{ color: '#0f172a', mb: 0.5 }}>
                      {geo.stateName} ({geo.stateCode}) — {geo.counties.map((c) => c.countyName).join(', ')}
                    </Typography>
                  ))}
                </Box>
              )}
            </Box>
          </CardContent>
        </Card>
      )}
    </Box>
  )
}

export default ClientDashBoard
