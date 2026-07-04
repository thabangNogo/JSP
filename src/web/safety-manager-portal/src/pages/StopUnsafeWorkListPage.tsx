import { useQuery } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Card,
  Chip,
  CircularProgress,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import { safetyApi } from '../api/safetyApi';

function formatDate(value: string) {
  return new Date(value).toLocaleString();
}

export default function StopUnsafeWorkListPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['stop-unsafe-work'],
    queryFn: () => safetyApi.getStopUnsafeWork(),
  });

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Stop Unsafe Work</Typography>
      {isLoading && <CircularProgress />}
      {error && <Alert severity="error" sx={{ mb: 2 }}>{(error as Error).message}</Alert>}
      <Card>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Department</TableCell>
              <TableCell>Location</TableCell>
              <TableCell>Section</TableCell>
              <TableCell>Category</TableCell>
              <TableCell>Risk Level</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Description</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {(data ?? []).length === 0 && !isLoading && (
              <TableRow>
                <TableCell colSpan={8} align="center">No stop unsafe work reports recorded.</TableCell>
              </TableRow>
            )}
            {(data ?? []).map((row) => (
              <TableRow key={row.id} hover>
                <TableCell>{formatDate(row.createdDate)}</TableCell>
                <TableCell>{row.department}</TableCell>
                <TableCell>{row.location}</TableCell>
                <TableCell>{row.section}</TableCell>
                <TableCell>{row.category}</TableCell>
                <TableCell>{row.immediateRisk}</TableCell>
                <TableCell>
                  <Chip label={row.status} size="small" />
                </TableCell>
                <TableCell sx={{ maxWidth: 280, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                  {row.description}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>
    </Box>
  );
}
