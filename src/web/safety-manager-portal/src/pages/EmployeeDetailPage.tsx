import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Typography,
  Tabs,
  Tab,
  Card,
  CardContent,
  Grid,
  Chip,
  CircularProgress,
  Alert,
  Button,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  IconButton,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf';
import { employeesApi } from '../api/employeesApi';
import { useEmployeePpeHistory } from '../hooks/usePpe';

export default function EmployeeDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [tab, setTab] = useState(0);

  const { data: ppeHistory } = useEmployeePpeHistory(id);

  const { data, isLoading, error } = useQuery({
    queryKey: ['employee', id],
    queryFn: () => employeesApi.getById(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" py={8}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) return <Alert severity="error">{(error as Error).message}</Alert>;
  if (!data) return null;

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/employees')} sx={{ mb: 2 }}>
        Back to Employees
      </Button>

      <Box display="flex" alignItems="center" gap={2} mb={2}>
        <Typography variant="h4">
          {data.name} {data.surname}
        </Typography>
        <Chip label={data.status} color={data.status === 'Active' ? 'success' : 'default'} />
      </Box>

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
        <Tab label="Profile" />
        <Tab label="Assessments" />
        <Tab label="Near Misses" />
        <Tab label="Corrective Actions" />
        <Tab label="PPE" />
        <Tab label="Activity Timeline" />
      </Tabs>

      {tab === 0 && (
        <Card>
          <CardContent>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}><Detail label="Name" value={data.name} /></Grid>
              <Grid item xs={12} sm={6}><Detail label="Surname" value={data.surname} /></Grid>
              <Grid item xs={12} sm={6}><Detail label="Company Number" value={data.companyNumber} /></Grid>
              <Grid item xs={12} sm={6}><Detail label="Department" value={data.department} /></Grid>
              <Grid item xs={12} sm={6}><Detail label="Occupation" value={data.occupation} /></Grid>
              <Grid item xs={12} sm={6}>
                <Detail label="Date Joined" value={new Date(data.dateJoined).toLocaleDateString()} />
              </Grid>
              <Grid item xs={12} sm={6}><Detail label="Email" value={data.email} /></Grid>
            </Grid>
          </CardContent>
        </Card>
      )}

      {tab === 1 && (
        <Card>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Assessment Name</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Created</TableCell>
                <TableCell>Completed</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data.assessments.map((a) => (
                <TableRow key={a.id}>
                  <TableCell>{a.title}</TableCell>
                  <TableCell>{a.status}</TableCell>
                  <TableCell>{new Date(a.createdDate).toLocaleDateString()}</TableCell>
                  <TableCell>
                    {a.completedDate ? new Date(a.completedDate).toLocaleDateString() : '—'}
                  </TableCell>
                  <TableCell>
                    <IconButton size="small" title="Export PDF (placeholder)">
                      <PictureAsPdfIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {data.assessments.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} align="center">No assessments yet.</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Card>
      )}

      {tab === 2 && (
        <Card>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>Category</TableCell>
                <TableCell>Location</TableCell>
                <TableCell>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data.nearMisses.map((n) => (
                <TableRow key={n.id}>
                  <TableCell>{new Date(n.date).toLocaleDateString()}</TableCell>
                  <TableCell>{n.category}</TableCell>
                  <TableCell>{n.location}</TableCell>
                  <TableCell>{n.status}</TableCell>
                </TableRow>
              ))}
              {data.nearMisses.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4} align="center">No near miss reports.</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Card>
      )}

      {tab === 3 && (
        <Card>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Description</TableCell>
                <TableCell>Assigned</TableCell>
                <TableCell>Due</TableCell>
                <TableCell>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data.correctiveActions.map((a) => (
                <TableRow key={a.id}>
                  <TableCell>{a.description}</TableCell>
                  <TableCell>{new Date(a.assignedDate).toLocaleDateString()}</TableCell>
                  <TableCell>{new Date(a.dueDate).toLocaleDateString()}</TableCell>
                  <TableCell>{a.status}</TableCell>
                </TableRow>
              ))}
              {data.correctiveActions.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4} align="center">No corrective actions assigned.</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Card>
      )}

      {tab === 4 && (
        <Card>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>PPE Item</TableCell>
                <TableCell>Requested</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Dispatch</TableCell>
                <TableCell>Collected</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {(ppeHistory ?? []).map((p) => (
                <TableRow key={p.requestId}>
                  <TableCell>{p.ppeItemName}</TableCell>
                  <TableCell>{new Date(p.requestedDate).toLocaleDateString()}</TableCell>
                  <TableCell>{p.status}</TableCell>
                  <TableCell>{p.dispatchDate ? new Date(p.dispatchDate).toLocaleDateString() : '-'}</TableCell>
                  <TableCell>{p.collectedDate ? new Date(p.collectedDate).toLocaleDateString() : '-'}</TableCell>
                </TableRow>
              ))}
              {(ppeHistory ?? []).length === 0 && (
                <TableRow><TableCell colSpan={5} align="center">No PPE history.</TableCell></TableRow>
              )}
            </TableBody>
          </Table>
        </Card>
      )}

      {tab === 5 && (
        <Card>
          <CardContent>
            {data.activityTimeline.map((item, i) => (
              <Box key={i} sx={{ py: 1.5, borderBottom: '1px solid', borderColor: 'divider' }}>
                <Typography variant="caption" color="text.secondary">
                  {new Date(item.occurredAt).toLocaleString()} · {item.activityType}
                </Typography>
                <Typography>{item.description}</Typography>
              </Box>
            ))}
            {data.activityTimeline.length === 0 && (
              <Typography color="text.secondary">No activity recorded.</Typography>
            )}
          </CardContent>
        </Card>
      )}
    </Box>
  );
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <Box>
      <Typography variant="caption" color="text.secondary">{label}</Typography>
      <Typography fontWeight={600}>{value || '—'}</Typography>
    </Box>
  );
}
