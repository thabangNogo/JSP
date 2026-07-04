import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Typography,
  Card,
  Button,
  CircularProgress,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { safetyApi } from '../../api/safetyApi';

export default function InjuryListPage() {
  const navigate = useNavigate();
  const { data, isLoading, error } = useQuery({
    queryKey: ['injuries'],
    queryFn: () => safetyApi.getInjuries(),
  });

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h4">Injury Register</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => navigate('/injuries/new')}>
          Capture Injury
        </Button>
      </Box>
      {isLoading && <CircularProgress />}
      {error && <Alert severity="error">{(error as Error).message}</Alert>}
      <Card>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Employee</TableCell>
              <TableCell>Department</TableCell>
              <TableCell>Location</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Status</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {(data ?? []).map((row: Record<string, unknown>) => (
              <TableRow
                key={String(row.id)}
                hover
                sx={{ cursor: 'pointer' }}
                onClick={() => navigate(`/injuries/${row.id}`)}
              >
                <TableCell>{String(row.employeeName)}</TableCell>
                <TableCell>{String(row.department)}</TableCell>
                <TableCell>{String(row.location)}</TableCell>
                <TableCell>{String(row.injuryType)}</TableCell>
                <TableCell>{String(row.status)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>
    </Box>
  );
}
