// import {
//   Box,
//   Card,
//   CardContent,
//   Divider,
//   FormControl,
//   InputLabel,
//   MenuItem,
//   Select,
//   Skeleton,
//   Typography,
// } from '@mui/material'
// import { useClient } from '../../../contexts'

import { useEffect, useMemo, useState, type FormEvent } from 'react'
import {
  Box,
  Button,
  Chip,
  Divider,
  List,
  ListItem,
  ListItemText,
  Paper,
  TextField,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CodeIcon from '@mui/icons-material/Code'
import DnsIcon from '@mui/icons-material/Dns'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import SearchIcon from '@mui/icons-material/Search'
import StorageIcon from '@mui/icons-material/Storage'
import CloseIcon from '@mui/icons-material/Close'
import { DivIcon } from 'leaflet'
import type { LatLngExpression } from 'leaflet'
import {
  LayersControl,
  MapContainer,
  Marker,
  Polygon,
  Popup,
  ScaleControl,
  TileLayer,
  Tooltip,
  useMap,
} from 'react-leaflet'
import { useClient } from '../../../contexts'
import 'leaflet/dist/leaflet.css'
import {
  fetchPropertyGeoData,
  type PropertyGeoData,
  type PropertyGeoDataRequest,
  type StarterPackage,
  type StarterIssue,
} from '../../../api/GeoData'
import StarterWorkspace from './StarterWorkspace'

import './propertySearch.css'

const defaultMapCenter: LatLngExpression = [12.2958, 76.6394]

const propertyMarker = new DivIcon({
  className: 'ps-leaflet-marker',
  html: '<span></span>',
  iconSize: [34, 44],
  iconAnchor: [17, 42],
  popupAnchor: [0, -38],
})

function MapResizeHandler() {
  const map = useMap()

  useEffect(() => {
    const resizeTimer = window.setTimeout(() => {
      map.invalidateSize()
    }, 0)

    return () => window.clearTimeout(resizeTimer)
  }, [map])

  return null
}

interface MapViewSyncProps {
  center: LatLngExpression
  parcelBoundary: LatLngExpression[]
}

function MapViewSync({ center, parcelBoundary }: MapViewSyncProps) {
  const map = useMap()

  useEffect(() => {
    if (parcelBoundary.length > 0) {
      map.fitBounds(parcelBoundary as [number, number][], { padding: [60, 60], maxZoom: 18 })
      return
    }

    map.setView(center, 13)
  }, [center, map, parcelBoundary])

  return null
}

function toLeafletPolygon(property: PropertyGeoData | null): LatLngExpression[] {
  const ring = property?.polygon?.coordinates?.[0]

  if (!ring) {
    return []
  }

  return ring.map(([longitude, latitude]) => [latitude, longitude] as [number, number])
}

function getPropertyCenter(property: PropertyGeoData | null): LatLngExpression {
  if (!property) {
    return defaultMapCenter
  }

  return [property.latitude, property.longitude]
}

interface PropertyLeafletMapProps {
  property: PropertyGeoData | null
  parcelBoundary: LatLngExpression[]
  onMarkerClick?: () => void
}

function PropertyLeafletMap({ property, parcelBoundary, onMarkerClick }: PropertyLeafletMapProps) {
  const center = getPropertyCenter(property)

  return (
    <Box className="ps-leaflet-shell">
      <MapContainer
        center={center}
        zoom={property ? 17 : 13}
        minZoom={12}
        maxZoom={19}
        scrollWheelZoom
        className="ps-leaflet-map"
      >
        <MapResizeHandler />
        <MapViewSync center={center} parcelBoundary={parcelBoundary} />
        <LayersControl position="topright">
          <LayersControl.BaseLayer checked name="Street Map">
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
          </LayersControl.BaseLayer>
          <LayersControl.BaseLayer name="Satellite Style">
            <TileLayer
              attribution='Tiles &copy; Esri'
              url="https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}"
            />
          </LayersControl.BaseLayer>
          {parcelBoundary.length > 0 && (
            <LayersControl.Overlay checked name="Parcel Boundary">
              <Polygon
                positions={parcelBoundary}
                pathOptions={{ color: '#facc15', fillColor: '#facc15', fillOpacity: 0.48, weight: 4 }}
              >
                <Tooltip permanent direction="top" offset={[42, -18]} className="ps-parcel-tooltip">
                  {property?.parcelId || property?.apn || 'Property'}
                </Tooltip>
              </Polygon>
            </LayersControl.Overlay>
          )}
        </LayersControl>
        {property && (
          <Marker position={center} icon={propertyMarker} eventHandlers={{ click: () => onMarkerClick?.() }}>
            <Popup>
              <strong>{property.parcelId}</strong>
              <br />
              {property.address}
            </Popup>
          </Marker>
        )}
        <ScaleControl position="bottomleft" metric imperial={false} />
      </MapContainer>
      {property && (
        <Box className="ps-map-inset">
        <MapContainer
          key={`${property.id}-${property.latitude}-${property.longitude}`}
          center={center}
          zoom={11}
          zoomControl={false}
          dragging={false}
          scrollWheelZoom={false}
          className="ps-inset-map"
        >
          <TileLayer
            attribution=""
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <Marker position={center} icon={propertyMarker} />
        </MapContainer>
        <Box className="ps-inset-label">{property.countyName}</Box>
        </Box>
      )}
    </Box>
  )
}

function PropertySearch() {
  const [parcelId, setParcelId] = useState('')
  const [propertyAddress, setPropertyAddress] = useState('')
  const [property, setProperty] = useState<PropertyGeoData | null>(null)
  const [starterPackage, setStarterPackage] = useState<StarterPackage | null>(null)
  const [starterIssues, setStarterIssues] = useState<StarterIssue[]>([])
  const [recentSearches, setRecentSearches] = useState<string[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [showStarterDialog, setShowStarterDialog] = useState(false)
  const [selectedIssue, setSelectedIssue] = useState<StarterIssue | null>(null)
  const [showMarkerDetails, setShowMarkerDetails] = useState(false)
  const [showWorkspace, setShowWorkspace] = useState(false)

  const { clientDetails, selectedCompanyId, selectedBranchId } = useClient()
  const selectedCompany = clientDetails?.companies.find((c) => c.companyId === selectedCompanyId) || null
  const selectedBranch = selectedCompany?.branches.find((b) => b.branchId === selectedBranchId) || null

  const branchHasAccessTo = (stateName?: string, countyName?: string) => {
    if (!selectedBranch) {
      return true
    }

    if (!stateName || !countyName) {
      return false
    }

    const normalizedState = stateName.trim().toLowerCase()
    const normalizedCounty = countyName.trim().toLowerCase()

    return selectedBranch.geoAccess.some((geo) =>
      geo.stateName.trim().toLowerCase() === normalizedState &&
      geo.counties.some((county) => county.countyName.trim().toLowerCase() === normalizedCounty)
    )
  }

  const parcelBoundary = useMemo(() => toLeafletPolygon(property), [property])

  const handleMarkerClick = () => {
    setShowMarkerDetails(true)
  }

  const handleSearch = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const trimmedParcelId = parcelId.trim()

    if (!trimmedParcelId) {
      setError('Enter a Parcel ID.')
      return
    }

    const request: PropertyGeoDataRequest = {
      parcelId: trimmedParcelId,
    }

    setLoading(true)
    setError('')
    setShowWorkspace(false)
    setShowStarterDialog(false)
    setShowMarkerDetails(false)
    setSelectedIssue(null)

    try {
      const result = await fetchPropertyGeoData(request)

      if (!result.parcel) {
        setProperty(null)
        setStarterPackage(null)
        setStarterIssues([])
        setPropertyAddress('')
        setError('Parcel not found')
        return
      }

      if (!branchHasAccessTo(result.parcel.stateName, result.parcel.countyName)) {
        setProperty(null)
        setStarterPackage(null)
        setStarterIssues([])
        setPropertyAddress('')
        setError('Access denied to this parcel. The selected company/branch does not have geographic access for this county.')
        return
      }

      setProperty(result.parcel)
      setStarterPackage(result.starterPackage || null)
      setStarterIssues(result.starterIssues || [])
      setParcelId(result.parcel.parcelId || trimmedParcelId)
      setPropertyAddress(result.parcel.address || '')
      setRecentSearches((current) => {
        const searchLabel = result.parcel.parcelId
        return [searchLabel, ...current.filter((item) => item !== searchLabel)].slice(0, 3)
      })
    } catch (searchError) {
      console.error('Failed to fetch property geo data:', searchError)
      setError('Parcel not found')
      setProperty(null)
      setStarterPackage(null)
      setStarterIssues([])
      setPropertyAddress('')
    } finally {
      setLoading(false)
    }
  }

  return (
    <>
      {showWorkspace && starterPackage ? (
        <StarterWorkspace
          starterJson={starterPackage.starterJson}
          parcelId={property?.parcelId || ''}
          county={property?.countyName || ''}
          state={property?.stateName || ''}
          onBack={() => setShowWorkspace(false)}
        />
      ) : (
        <>
      <Box className="ps-main-card">
        <Box className="property-search-root">
        <Box className="ps-left">
          <Typography variant="h6" className="ps-title">
            Search Property
          </Typography>

          <Box component="form" className="ps-form" noValidate autoComplete="off" onSubmit={handleSearch}>
            <TextField
              label="Parcel ID"
              variant="outlined"
              size="small"
              value={parcelId}
              onChange={(event) => setParcelId(event.target.value)}
              fullWidth
            />
            <TextField
              label="Property Address"
              variant="outlined"
              size="small"
              value={propertyAddress}
              fullWidth
              disabled
            />
            <Button
              type="submit"
              variant="contained"
              color="primary"
              className="ps-search-btn"
              startIcon={<SearchIcon />}
              disabled={loading}
            >
              {loading ? 'Searching...' : 'Search'}
            </Button>
            {error && <Typography className="ps-error">{error}</Typography>}
          </Box>

          <Divider sx={{ my: 2 }} />

          <Typography variant="subtitle2" className="ps-subtitle">
            Recent Searches
          </Typography>
          <List className="ps-recent-list">
            {recentSearches.length > 0 ? (
              recentSearches.map((search) => (
                <ListItem key={search}>
                  <ListItemText primary={search} />
                </ListItem>
              ))
            ) : (
              <ListItem>
                <ListItemText primary="No recent searches" />
              </ListItem>
            )}
          </List>
        </Box>

        <Box className="ps-center">
          <Paper className="ps-map-placeholder" elevation={0}>
            <Box id="property-map-root" className="ps-map-integration-slot">
              <PropertyLeafletMap property={property} parcelBoundary={parcelBoundary} onMarkerClick={handleMarkerClick} />
            </Box>
          </Paper>
        </Box>

        <Box className="ps-right">
          <Paper className="ps-details" elevation={0}>
            {!property ? (
              <Typography color="text.secondary">
                Search by Parcel ID to load property details.
              </Typography>
            ) : !showMarkerDetails ? (
              <Box>
                <Typography variant="h6" sx={{ fontWeight: 800, mb: 2 }}>
                  Property Details
                </Typography>
                <Typography variant="subtitle2" color="text.secondary">
                  Parcel ID
                </Typography>
                <Typography variant="h6" sx={{ fontWeight: 700, mb: 2 }}>
                  {property.parcelId}
                </Typography>

                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    APN
                  </Typography>
                  <Typography variant="body1">{property.apn}</Typography>
                </Box>

                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Address
                  </Typography>
                  <Typography variant="body1">{property.address}</Typography>
                </Box>

                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    County
                  </Typography>
                  <Typography variant="body1">{property.countyName}</Typography>
                </Box>

                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    State
                  </Typography>
                  <Typography variant="body1">{property.stateName}</Typography>
                </Box>

                <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Has Starter
                  </Typography>
                  <Chip label={property.hasStarter ? 'YES' : 'NO'} color={property.hasStarter ? 'success' : 'default'} size="small" />
                </Box>

                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Starter Years
                  </Typography>
                  <Typography variant="body1">
                    {property.hasStarter ? `${property.starterFromYear} - ${property.starterToYear}` : 'Not available'}
                  </Typography>
                </Box>

                <Box sx={{ mb: 3 }}>
                  <Typography variant="body2" color="text.secondary">
                    Coordinates
                  </Typography>
                  <Typography variant="body1">
                    {property.latitude}, {property.longitude}
                  </Typography>
                </Box>

                {starterPackage && (
                  <>
                    <Divider sx={{ my: 2 }} />
                    <Typography variant="h6" sx={{ fontWeight: 800, mb: 2 }}>
                      Starter Package
                    </Typography>

                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Type
                      </Typography>
                      <Typography variant="body1">{starterPackage.starterType}</Typography>
                    </Box>

                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Status
                      </Typography>
                      <Chip
                        label={starterPackage.status}
                        color={starterPackage.status === 'Completed' ? 'success' : 'warning'}
                        size="small"
                      />
                    </Box>

                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Coverage Years
                      </Typography>
                      <Typography variant="body1">
                        {starterPackage.coverageFromYear} - {starterPackage.coverageToYear}
                      </Typography>
                    </Box>

                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Risk Score
                      </Typography>
                      <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                        <Typography variant="body1" sx={{ fontWeight: 700 }}>
                          {starterPackage.riskScore}
                        </Typography>
                        <Chip
                          label={starterPackage.riskLevel}
                          color={
                            starterPackage.riskLevel === 'High'
                              ? 'error'
                              : starterPackage.riskLevel === 'Medium'
                                ? 'warning'
                                : 'success'
                          }
                          size="small"
                        />
                      </Box>
                    </Box>

                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Current Owner
                      </Typography>
                      <Typography variant="body1">{starterPackage.currentOwner}</Typography>
                    </Box>

                    <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Insurance Ready
                      </Typography>
                      <Chip
                        label={starterPackage.insuranceReady ? 'YES' : 'NO'}
                        color={starterPackage.insuranceReady ? 'success' : 'default'}
                        size="small"
                      />
                    </Box>

                    <Button
                      variant="outlined"
                      size="small"
                      fullWidth
                      onClick={() => setShowStarterDialog(true)}
                      sx={{ mb: 2 }}
                    >
                      View Starter JSON
                    </Button>

                    {starterIssues.length > 0 && (
                      <>
                        <Divider sx={{ my: 2 }} />
                        <Typography variant="h6" sx={{ fontWeight: 800, mb: 1 }}>
                          Issues ({starterIssues.length})
                        </Typography>
                        <Box sx={{ maxHeight: 200, overflowY: 'auto' }}>
                          {starterIssues.map((issue) => (
                            <Box
                              key={issue.id}
                              sx={{
                                p: 1.5,
                                mb: 1,
                                border: '1px solid #e5e7eb',
                                borderRadius: 1,
                                cursor: 'pointer',
                                '&:hover': { bgcolor: '#f9fafb' },
                              }}
                              onClick={() => setSelectedIssue(issue)}
                            >
                              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 0.5 }}>
                                <Typography variant="body2" sx={{ fontWeight: 700 }}>
                                  {issue.issueType}
                                </Typography>
                                <Chip
                                  label={issue.severity}
                                  size="small"
                                  color={issue.severity === 'HIGH' ? 'error' : issue.severity === 'MEDIUM' ? 'warning' : 'success'}
                                />
                              </Box>
                              <Typography variant="caption" color="text.secondary">
                                {issue.description}
                              </Typography>
                            </Box>
                          ))}
                        </Box>
                      </>
                    )}
                  </>
                )}

                <Button
                  variant="contained"
                  color="primary"
                  fullWidth
                  className="ps-workspace-btn"
                  disabled={!property.hasStarter}
                  onClick={() => setShowWorkspace(true)}
                >
                  Open Starter Workspace
                </Button>
              </Box>
            ) : (
              <Box>
                <Button
                  onClick={() => setShowMarkerDetails(false)}
                  sx={{ mb: 2, textTransform: 'none' }}
                  variant="text"
                  size="small"
                >
                  ← Back
                </Button>

                <Typography variant="h6" sx={{ fontWeight: 800, mb: 2 }}>
                  Starter Details
                </Typography>

                {starterPackage && (
                  <>
                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Type
                      </Typography>
                      <Typography variant="body1">{starterPackage.starterType}</Typography>
                    </Box>

                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Current Owner
                      </Typography>
                      <Typography variant="body1" sx={{ fontWeight: 700 }}>
                        {starterPackage.currentOwner}
                      </Typography>
                    </Box>

                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Status
                      </Typography>
                      <Chip
                        label={starterPackage.status}
                        color={starterPackage.status === 'Completed' ? 'success' : 'warning'}
                        size="small"
                      />
                    </Box>

                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Risk Level
                      </Typography>
                      <Chip
                        label={starterPackage.riskLevel}
                        color={
                          starterPackage.riskLevel === 'High'
                            ? 'error'
                            : starterPackage.riskLevel === 'Medium'
                              ? 'warning'
                              : 'success'
                        }
                        size="small"
                      />
                    </Box>

                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Coverage Years
                      </Typography>
                      <Typography variant="body1">
                        {starterPackage.coverageFromYear} - {starterPackage.coverageToYear}
                      </Typography>
                    </Box>

                    <Divider sx={{ my: 2 }} />

                    {starterIssues.length > 0 && (
                      <>
                        <Typography variant="subtitle2" sx={{ fontWeight: 800, mb: 1 }}>
                          Issues ({starterIssues.length})
                        </Typography>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                          {starterIssues.map((issue) => (
                            <Box
                              key={issue.id}
                              sx={{
                                p: 1,
                                border: '1px solid #e5e7eb',
                                borderRadius: 1,
                                cursor: 'pointer',
                                '&:hover': { bgcolor: '#f9fafb' },
                              }}
                              onClick={() => setSelectedIssue(issue)}
                            >
                              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 0.5 }}>
                                <Typography variant="caption" sx={{ fontWeight: 700 }}>
                                  {issue.issueType}
                                </Typography>
                                <Chip
                                  label={issue.severity}
                                  size="small"
                                  color={issue.severity === 'HIGH' ? 'error' : issue.severity === 'MEDIUM' ? 'warning' : 'success'}
                                />
                              </Box>
                              <Typography variant="caption" color="text.secondary">
                                {issue.description}
                              </Typography>
                            </Box>
                          ))}
                        </Box>
                      </>
                    )}
                  </>
                )}
              </Box>
            )}
          </Paper>
        </Box>
        </Box>
      </Box>

      {/* Footer / Info cards below the main map area */}
      <Box className="ps-footer">
        <Paper className="ps-footer-card">
          <Typography variant="h6" sx={{ fontWeight: 700, mb: 1 }}>
            How Map Renders Polygon?
          </Typography>
          <Box className="ps-steps">
            <Box className="ps-step">
              <StorageIcon className="ps-step-icon ps-step-blue" />
              <Typography sx={{ fontWeight: 700 }}>1. Parcel searched</Typography>
              <Typography color="text.secondary">You enter Parcel ID / APN.</Typography>
            </Box>
            <Box className="ps-step">
              <DnsIcon className="ps-step-icon ps-step-purple" />
              <Typography sx={{ fontWeight: 700 }}>2. System fetches geometry</Typography>
              <Typography color="text.secondary">Backend gets polygon (from DB / GIS).</Typography>
            </Box>
            <Box className="ps-step">
              <CodeIcon className="ps-step-icon ps-step-green" />
              <Typography sx={{ fontWeight: 700 }}>3. Map renders polygon</Typography>
              <Typography color="text.secondary">Polygon is drawn on map using coordinates.</Typography>
            </Box>
            <Box className="ps-step">
              <LocationOnIcon className="ps-step-icon ps-step-red" />
              <Typography sx={{ fontWeight: 700 }}>4. Location identified</Typography>
              <Typography color="text.secondary">Map zooms to parcel and shows exact boundary.</Typography>
            </Box>
          </Box>
        </Paper>

        <Paper className="ps-footer-card">
          <Typography variant="h6" sx={{ fontWeight: 700, mb: 1 }}>
            Use of Polygon on Map
          </Typography>
          <Box className="ps-uses">
            {["Shows exact property boundary","Helps verify location visually","Checks if property lies within allowed branch area","Validates county / state jurisdiction","Supports workflow decisions (authority, routing, risk)","Foundation for future analysis (flood, zoning, risk, AI)"].map((t)=> (
              <Box key={t} className="ps-use-item">
                <CheckCircleIcon sx={{ color: '#059669', mr: 1, fontSize: 18 }} />
                <Typography>{t}</Typography>
              </Box>
            ))}
          </Box>
        </Paper>

        <Paper className="ps-legend-card">
          <Typography variant="h6" sx={{ fontWeight: 700, mb: 1 }}>
            Map Layer Explanation
          </Typography>
          <Box className="ps-legend">
            <Box className="ps-legend-item"><span className="legend-swatch swatch-yellow"/> Parcel Boundary (API Polygon)</Box>
            <Box className="ps-legend-item"><span className="legend-marker"/> Property Location</Box>
          </Box>
        </Paper>
      </Box>

      {/* Starter JSON Dialog */}
      <Dialog open={showStarterDialog} onClose={() => setShowStarterDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 700 }}>
          Starter JSON Details
          <Button
            onClick={() => setShowStarterDialog(false)}
            sx={{ position: 'absolute', right: 8, top: 8, minWidth: 'auto', p: 0.5 }}
          >
            <CloseIcon />
          </Button>
        </DialogTitle>
        <DialogContent dividers>
          {starterPackage && (
            <Box sx={{ overflow: 'auto', maxHeight: 400 }}>
              <pre style={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word', fontSize: '0.75rem' }}>
                {JSON.stringify(JSON.parse(starterPackage.starterJson), null, 2)}
              </pre>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowStarterDialog(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Issue Details Dialog */}
      <Dialog open={!!selectedIssue} onClose={() => setSelectedIssue(null)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 700 }}>
          Issue Details
          <Button
            onClick={() => setSelectedIssue(null)}
            sx={{ position: 'absolute', right: 8, top: 8, minWidth: 'auto', p: 0.5 }}
          >
            <CloseIcon />
          </Button>
        </DialogTitle>
        <DialogContent dividers>
          {selectedIssue && (
            <Box>
              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Issue Type
                </Typography>
                <Typography variant="body1" sx={{ fontWeight: 700 }}>
                  {selectedIssue.issueType}
                </Typography>
              </Box>

              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Description
                </Typography>
                <Typography variant="body1">{selectedIssue.description}</Typography>
              </Box>

              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Severity
                </Typography>
                <Chip
                  label={selectedIssue.severity}
                  color={selectedIssue.severity === 'HIGH' ? 'error' : selectedIssue.severity === 'MEDIUM' ? 'warning' : 'success'}
                  size="small"
                />
              </Box>

              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Status
                </Typography>
                <Chip
                  label={selectedIssue.resolved ? 'Resolved' : 'Open'}
                  color={selectedIssue.resolved ? 'success' : 'warning'}
                  size="small"
                />
              </Box>

              <Box>
                <Typography variant="body2" color="text.secondary">
                  Created Date
                </Typography>
                <Typography variant="body1">
                  {new Date(selectedIssue.createdDate).toLocaleDateString()}
                </Typography>
              </Box>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSelectedIssue(null)}>Close</Button>
        </DialogActions>
      </Dialog>
        </>
      )}
    </>
  )
}
  // const { clientDetails, selectedCompanyId, selectedBranchId, loadingDetails, setSelectedCompanyId, setSelectedBranchId } =
  //   useClient()

  // const selectedCompany = clientDetails?.companies.find((c) => c.companyId === selectedCompanyId) || null
  // const selectedBranch = selectedCompany?.branches.find((b) => b.branchId === selectedBranchId) || null

  // if (loadingDetails) {
  //   return (
  //     <Box sx={{ display: 'grid', gap: 3 }}>
  //       <Skeleton variant="rectangular" height={320} />
  //       <Skeleton variant="rectangular" height={200} />
  //     </Box>
  //   )
  // }

  // if (!clientDetails) {
  //   return (
  //     <Box sx={{ textAlign: 'center', py: 6 }}>
  //       <Typography color="text.secondary">Loading client details...</Typography>
  //     </Box>
  //   )
  // }

  // const handleCompanyChange = (companyId: number) => {
  //   setSelectedCompanyId(companyId)
  //   const company = clientDetails.companies.find((c) => c.companyId === companyId)
  //   if (company && company.branches.length > 0) {
  //     setSelectedBranchId(company.branches[0].branchId)
  //   }
  // }

  // const handleBranchChange = (branchId: number) => {
  //   setSelectedBranchId(branchId)
  // }

  // return (
  //   <Box sx={{ display: 'grid', gap: 3 }}>
  //     {/* Client Info Card */}
  //     <Card sx={{ borderRadius: 3, boxShadow: '0 10px 30px rgba(15,23,42,0.08)' }}>
  //       <CardContent sx={{ p: 4 }}>
  //         <Typography component="h2" variant="h5" sx={{ fontWeight: 700, mb: 3, color: '#0f172a' }}>
  //           Client Information
  //         </Typography>

  //         <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 3 }}>
  //           <Box>
  //             <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //               Client Name
  //             </Typography>
  //             <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
  //               {clientDetails.clientName}
  //             </Typography>
  //           </Box>

  //           <Box>
  //             <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //               Client Code
  //             </Typography>
  //             <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
  //               {clientDetails.clientCode}
  //             </Typography>
  //           </Box>

  //           <Box>
  //             <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //               Client Type
  //             </Typography>
  //             <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
  //               {clientDetails.clientType}
  //             </Typography>
  //           </Box>

  //           <Box>
  //             <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //               Email
  //             </Typography>
  //             <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
  //               {clientDetails.email}
  //             </Typography>
  //           </Box>

  //           <Box>
  //             <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //               Phone Number
  //             </Typography>
  //             <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
  //               {clientDetails.phoneNumber}
  //             </Typography>
  //           </Box>
  //         </Box>
  //       </CardContent>
  //     </Card>

  //     {/* Company & Branch Selection */}
  //     <Card sx={{ borderRadius: 3, boxShadow: '0 10px 30px rgba(15,23,42,0.08)' }}>
  //       <CardContent sx={{ p: 4 }}>
  //         <Typography component="h2" variant="h5" sx={{ fontWeight: 700, mb: 3, color: '#0f172a' }}>
  //           Company & Branch Selection
  //         </Typography>

  //         <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 3 }}>
  //           <FormControl fullWidth>
  //             <InputLabel id="company-select-label" sx={{ color: '#64748b' }}>
  //               Company
  //             </InputLabel>
  //             <Select
  //               labelId="company-select-label"
  //               id="company-select"
  //               value={selectedCompanyId || ''}
  //               label="Company"
  //               onChange={(e) => handleCompanyChange(e.target.value as number)}
  //               sx={{
  //                 '& .MuiOutlinedInput-root': {
  //                   color: '#0f172a',
  //                   '& fieldset': {
  //                     borderColor: 'rgba(15, 23, 42, 0.12)',
  //                   },
  //                   '&:hover fieldset': {
  //                     borderColor: 'rgba(79, 70, 229, 0.3)',
  //                   },
  //                 },
  //               }}
  //             >
  //               {clientDetails.companies.map((company) => (
  //                 <MenuItem key={company.companyId} value={company.companyId}>
  //                   {company.companyName}
  //                 </MenuItem>
  //               ))}
  //             </Select>
  //           </FormControl>

  //           <FormControl fullWidth disabled={!selectedCompanyId}>
  //             <InputLabel id="branch-select-label" sx={{ color: '#64748b' }}>
  //               Branch
  //             </InputLabel>
  //             <Select
  //               labelId="branch-select-label"
  //               id="branch-select"
  //               value={selectedBranchId || ''}
  //               label="Branch"
  //               onChange={(e) => handleBranchChange(e.target.value as number)}
  //               sx={{
  //                 '& .MuiOutlinedInput-root': {
  //                   color: '#0f172a',
  //                   '& fieldset': {
  //                     borderColor: 'rgba(15, 23, 42, 0.12)',
  //                   },
  //                   '&:hover fieldset': {
  //                     borderColor: 'rgba(79, 70, 229, 0.3)',
  //                   },
  //                 },
  //               }}
  //             >
  //               {selectedCompany?.branches.map((branch) => (
  //                 <MenuItem key={branch.branchId} value={branch.branchId}>
  //                   {branch.branchName} ({branch.branchCode})
  //                 </MenuItem>
  //               ))}
  //             </Select>
  //           </FormControl>
  //         </Box>
  //       </CardContent>
  //     </Card>

  //     {/* Branch Details */}
  //     {selectedBranch && (
  //       <Card sx={{ borderRadius: 3, boxShadow: '0 10px 30px rgba(15,23,42,0.08)' }}>
  //         <CardContent sx={{ p: 4 }}>
  //           <Typography component="h2" variant="h5" sx={{ fontWeight: 700, mb: 3, color: '#0f172a' }}>
  //             Selected Branch Details
  //           </Typography>

  //           <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 3 }}>
  //             <Box>
  //               <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //                 Branch Name
  //               </Typography>
  //               <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
  //                 {selectedBranch.branchName}
  //               </Typography>
  //             </Box>

  //             <Box>
  //               <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //                 Branch Code
  //               </Typography>
  //               <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
  //                 {selectedBranch.branchCode}
  //               </Typography>
  //             </Box>

  //             <Box>
  //               <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //                 Branch Type
  //               </Typography>
  //               <Typography variant="body1" sx={{ color: '#0f172a', fontWeight: 500 }}>
  //                 {selectedBranch.branchType}
  //               </Typography>
  //             </Box>

  //             <Box>
  //               <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //                 Can Load Starter
  //               </Typography>
  //               <Typography variant="body1" sx={{ color: selectedBranch.canLoadStarter ? '#059669' : '#dc2626', fontWeight: 500 }}>
  //                 {selectedBranch.canLoadStarter ? 'Yes' : 'No'}
  //               </Typography>
  //             </Box>

  //             <Box>
  //               <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //                 Can Review Title
  //               </Typography>
  //               <Typography variant="body1" sx={{ color: selectedBranch.canReviewTitle ? '#059669' : '#dc2626', fontWeight: 500 }}>
  //                 {selectedBranch.canReviewTitle ? 'Yes' : 'No'}
  //               </Typography>
  //             </Box>

  //             <Box>
  //               <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mb: 0.5 }}>
  //                 Can Issue Insurance
  //               </Typography>
  //               <Typography variant="body1" sx={{ color: selectedBranch.canIssueInsurance ? '#059669' : '#dc2626', fontWeight: 500 }}>
  //                 {selectedBranch.canIssueInsurance ? 'Yes' : 'No'}
  //               </Typography>
  //             </Box>

  //             {selectedBranch.geoAccess.length > 0 && (
  //               <Box sx={{ gridColumn: '1 / -1' }}>
  //                 <Divider sx={{ my: 1 }} />
  //                 <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600, mt: 2, mb: 1 }}>
  //                   Geographic Access
  //                 </Typography>
  //                 {selectedBranch.geoAccess.map((geo) => (
  //                   <Typography key={geo.stateCode} variant="body2" sx={{ color: '#0f172a', mb: 0.5 }}>
  //                     {geo.stateName} ({geo.stateCode}) — {geo.counties.map((c) => c.countyName).join(', ')}
  //                   </Typography>
  //                 ))}
  //               </Box>
  //             )}
  //           </Box>
  //         </CardContent>
  //       </Card>
  //     )}
  //   </Box>
  //)


export default PropertySearch
