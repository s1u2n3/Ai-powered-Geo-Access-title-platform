import {
  Box,
  Button,
  Card,
  CardContent,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Slider,
  TextField,
  Typography,
} from '@mui/material'
import CloudUploadIcon from '@mui/icons-material/CloudUpload'
import ContentCutIcon from '@mui/icons-material/ContentCut'

function DocumentChunk() {
  return (
    <Box sx={{ display: 'grid', gap: 3 }}>
      <Box>
        <Typography component="h1" variant="h5" sx={{ fontWeight: 800, color: '#0f172a' }}>
          Document Chunk
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          Prepare uploaded document text for indexing and AI search.
        </Typography>
      </Box>

      <Card sx={{ borderRadius: 2, border: '1px solid rgba(15,23,42,0.08)', boxShadow: '0 18px 45px rgba(15,23,42,0.06)' }}>
        <CardContent sx={{ p: 3 }}>
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 2, mb: 3 }}>
            <TextField label="Document Name" size="small" fullWidth />
            <FormControl size="small" fullWidth>
              <InputLabel id="document-type-label">Document Type</InputLabel>
              <Select labelId="document-type-label" label="Document Type" defaultValue="Encumbrance Certificate">
                <MenuItem value="Encumbrance Certificate">Encumbrance Certificate</MenuItem>
                <MenuItem value="Sale Deed">Sale Deed</MenuItem>
                <MenuItem value="Mortgage Deed">Mortgage Deed</MenuItem>
                <MenuItem value="KYC">KYC</MenuItem>
              </Select>
            </FormControl>
          </Box>

          <Button variant="outlined" component="label" startIcon={<CloudUploadIcon />} sx={{ mb: 3 }}>
            Upload Document
            <input hidden type="file" />
          </Button>

          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 3, mb: 3 }}>
            <Box>
              <Typography variant="body2" sx={{ fontWeight: 700, mb: 1 }}>
                Chunk Size
              </Typography>
              <Slider defaultValue={800} step={100} min={200} max={2000} valueLabelDisplay="auto" />
            </Box>
            <Box>
              <Typography variant="body2" sx={{ fontWeight: 700, mb: 1 }}>
                Overlap
              </Typography>
              <Slider defaultValue={120} step={20} min={0} max={400} valueLabelDisplay="auto" />
            </Box>
          </Box>

          <TextField
            label="Extracted Text"
            minRows={8}
            multiline
            fullWidth
            placeholder="Paste document text here to preview chunking."
          />

          <Box sx={{ mt: 3, display: 'flex', justifyContent: 'flex-end' }}>
            <Button variant="contained" startIcon={<ContentCutIcon />}>
              Create Chunks
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  )
}

export default DocumentChunk
