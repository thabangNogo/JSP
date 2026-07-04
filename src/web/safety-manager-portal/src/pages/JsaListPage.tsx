import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Card,
  Chip,
  CircularProgress,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { safetyApi } from '../api/safetyApi';
import type { JobSafetyAssessment } from '../types/safety';

const STATUS_OPTIONS = ['All', 'Submitted', 'Approved', 'Rejected', 'InReview'] as const;

function statusColor(status: string): 'default' | 'success' | 'error' | 'info' | 'warning' {
  switch (status) {
    case 'Approved': return 'success';
    case 'Rejected': return 'error';
    case 'Submitted': return 'info';
    case 'InReview': return 'warning';
    default: return 'default';
  }
}

function formatDate(value?: string) {
  if (!value) return '—';
  return new Date(value).toLocaleString();
}

export default function JsaListPage() {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState<string>('All');

  const { data, isLoading, error } = useQuery({
    queryKey: ['jsa-reports', statusFilter],
    queryFn: () =>
      safetyApi.getJsaReports(
        statusFilter !== 'All' ? { status: statusFilter } : undefined,
      ),
  });

  const rows = useMemo(
    () => (data ?? []).filter((j: JobSafetyAssessment) => j.status !== 'Draft'),
    [data],
  );

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h4">Job Safety Assessments</Typography>
        <TextField
          select
          size="small"
          label="Status"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          sx={{ minWidth: 180 }}
        >
          {STATUS_OPTIONS.map((s) => (
            <MenuItem key={s} value={s}>{s}</MenuItem>
          ))}
        </TextField>
      </Box>

      {isLoading && <CircularProgress />}
      {error && <Alert severity="error" sx={{ mb: 2 }}>{(error as Error).message}</Alert>}

      <Card>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Title</TableCell>
              <TableCell>Department</TableCell>
              <TableCell>Location</TableCell>
              <TableCell>Section</TableCell>
              <TableCell>Employee</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Last Saved</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.length === 0 && !isLoading && (
              <TableRow>
                <TableCell colSpan={7} align="center">No assessments found.</TableCell>
              </TableRow>
            )}
            {rows.map((row: JobSafetyAssessment) => (
              <TableRow
                key={row.id}
                hover
                sx={{ cursor: row.status === 'Submitted' ? 'pointer' : 'default' }}
                onClick={() => row.status === 'Submitted' && navigate(`/jsas/${row.id}/review`)}
              >
                <TableCell>{row.title}</TableCell>
                <TableCell>{row.department}</TableCell>
                <TableCell>{row.location}</TableCell>
                <TableCell>{row.section}</TableCell>
                <TableCell>
                  {[row.signOffName, row.signOffSurname].filter(Boolean).join(' ') || '—'}
                </TableCell>
                <TableCell>
                  <Chip label={row.status} size="small" color={statusColor(row.status)} />
                </TableCell>
                <TableCell>{formatDate(row.lastSavedAt ?? row.validFrom)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>
    </Box>
  );
}
