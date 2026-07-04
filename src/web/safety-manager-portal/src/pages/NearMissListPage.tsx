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

export default function NearMissListPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['near-misses'],
    queryFn: () => safetyApi.getNearMisses(),
  });

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Near Misses</Typography>
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
              <TableCell>Status</TableCell>
              <TableCell>Description</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {(data ?? []).length === 0 && !isLoading && (
              <TableRow>
                <TableCell colSpan={7} align="center">No near misses recorded.</TableCell>
              </TableRow>
            )}
            {(data ?? []).map((row) => (
              <TableRow key={row.id} hover>
                <TableCell>{formatDate(row.occurredAt)}</TableCell>
                <TableCell>{row.department}</TableCell>
                <TableCell>{row.location}</TableCell>
                <TableCell>{row.section}</TableCell>
                <TableCell>{row.category}</TableCell>
                <TableCell>
                  <Chip label={row.status} size="small" />
                </TableCell>
                <TableCell sx={{ maxWidth: 320, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
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
