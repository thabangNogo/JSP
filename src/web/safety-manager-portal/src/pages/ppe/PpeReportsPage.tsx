import { Box, Typography, Card, Table, TableHead, TableRow, TableCell, TableBody, CircularProgress, Alert } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { ppeApi } from '../../api/ppeApi';

export default function PpeReportsPage() {
  const outstanding = useQuery({ queryKey: ['ppe', 'report', 'outstanding'], queryFn: () => ppeApi.reportOutstanding() });
  const overdue = useQuery({ queryKey: ['ppe', 'report', 'overdue'], queryFn: () => ppeApi.reportOverdue() });
  const byDept = useQuery({ queryKey: ['ppe', 'report', 'dept'], queryFn: () => ppeApi.reportByDepartment() });
  const trends = useQuery({ queryKey: ['ppe', 'report', 'trends'], queryFn: () => ppeApi.reportUsageTrends() });

  const loading = outstanding.isLoading || overdue.isLoading;

  return (
    <Box>
      <Typography variant="h4" mb={2}>PPE Reports</Typography>
      {loading && <CircularProgress />}
      {outstanding.error && <Alert severity="error">{(outstanding.error as Error).message}</Alert>}

      <Typography variant="h6" mt={2} mb={1}>Outstanding PPE Requests</Typography>
      <ReportTable rows={outstanding.data ?? []} columns={['requestNumber', 'employeeName', 'ppeItemName', 'status']} />

      <Typography variant="h6" mt={3} mb={1}>Overdue PPE Requests</Typography>
      <ReportTable rows={overdue.data ?? []} columns={['requestNumber', 'employeeName', 'requiredByDate', 'status']} highlightOverdue />

      <Typography variant="h6" mt={3} mb={1}>PPE By Department</Typography>
      <Card sx={{ mb: 2 }}>
        <Table size="small">
          <TableHead><TableRow><TableCell>Department</TableCell><TableCell>Count</TableCell></TableRow></TableHead>
          <TableBody>
            {(byDept.data ?? []).map((r) => (
              <TableRow key={r.label}><TableCell>{r.label}</TableCell><TableCell>{r.count}</TableCell></TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>

      <Typography variant="h6" mt={2} mb={1}>PPE Usage Trends</Typography>
      <Card>
        <Table size="small">
          <TableHead><TableRow><TableCell>Month</TableCell><TableCell>Requests</TableCell></TableRow></TableHead>
          <TableBody>
            {(trends.data ?? []).map((r) => (
              <TableRow key={r.label}><TableCell>{r.label}</TableCell><TableCell>{r.count}</TableCell></TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>
    </Box>
  );
}

function ReportTable({ rows, columns, highlightOverdue }: { rows: Record<string, unknown>[]; columns: string[]; highlightOverdue?: boolean }) {
  return (
    <Card sx={{ mb: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>{columns.map((c) => <TableCell key={c}>{c}</TableCell>)}</TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={String(row.id)} sx={highlightOverdue ? { bgcolor: 'error.light' } : undefined}>
              {columns.map((c) => <TableCell key={c}>{String(row[c] ?? '')}</TableCell>)}
            </TableRow>
          ))}
          {rows.length === 0 && (
            <TableRow><TableCell colSpan={columns.length} align="center">No data</TableCell></TableRow>
          )}
        </TableBody>
      </Table>
    </Card>
  );
}
