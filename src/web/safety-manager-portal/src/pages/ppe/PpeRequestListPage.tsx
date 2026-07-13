import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Typography, Button, CircularProgress, Alert,
} from '@mui/material';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import AddIcon from '@mui/icons-material/Add';
import { usePpeRequests } from '../../hooks/usePpe';
import PpeRequestWorkflowActions from '../../components/ppe/PpeRequestWorkflowActions';

export default function PpeRequestListPage({ archivedOnly = false, issuedOnly = false }: { archivedOnly?: boolean; issuedOnly?: boolean }) {
  const navigate = useNavigate();
  const filter = archivedOnly ? { archivedOnly: true } : issuedOnly ? { issuedOnly: true } : { archivedOnly: false };
  const { data, isLoading, error } = usePpeRequests(filter);

  const columns: GridColDef[] = useMemo(() => [
    { field: 'requestNumber', headerName: 'Request #', flex: 1, minWidth: 130 },
    { field: 'employeeName', headerName: 'Employee', flex: 1, minWidth: 140 },
    { field: 'department', headerName: 'Department', flex: 1, minWidth: 120 },
    { field: 'ppeItemName', headerName: 'PPE Item', flex: 1, minWidth: 140 },
    { field: 'quantity', headerName: 'Qty', width: 70 },
    { field: 'priority', headerName: 'Priority', width: 100 },
    { field: 'status', headerName: 'Status', width: 130 },
    {
      field: 'requestedDate',
      headerName: 'Requested',
      width: 110,
      valueFormatter: (v) => new Date(String(v)).toLocaleDateString(),
    },
    {
      field: 'requiredByDate',
      headerName: 'Required',
      width: 110,
      valueFormatter: (v) => new Date(String(v)).toLocaleDateString(),
    },
    { field: 'ageDays', headerName: 'Age (Days)', width: 100 },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 360,
      sortable: false,
      renderCell: ({ row }) => (
        <Box display="flex" gap={0.5} flexWrap="wrap" alignItems="center" py={0.5}>
          <Button size="small" onClick={() => navigate(`/ppe/requests/${row.id}`)}>View</Button>
          {!archivedOnly && row.status === 'Requested' && (
            <Button size="small" onClick={() => navigate(`/ppe/requests/${row.id}/edit`)}>Edit</Button>
          )}
          {!archivedOnly && (
            <PpeRequestWorkflowActions
              variant="inline"
              requestId={String(row.id)}
              status={row.status}
              employeeName={row.employeeName}
            />
          )}
        </Box>
      ),
    },
  ], [archivedOnly, navigate]);

  const rows = (data ?? []).map((r) => ({
    ...r,
    id: r.id,
  }));

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h4">
          {archivedOnly ? 'Archived PPE Requests' : issuedOnly ? 'Issued PPE' : 'PPE Requests'}
        </Typography>
        {!archivedOnly && !issuedOnly && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => navigate('/ppe/requests/new')}>
            New Request
          </Button>
        )}
      </Box>
      {!archivedOnly && !issuedOnly && (
        <Alert severity="info" sx={{ mb: 2 }}>
          Safety Managers can approve or reject requests in <strong>Requested</strong> status.
          Open a request for the full workflow panel.
        </Alert>
      )}
      {isLoading && <CircularProgress />}
      {error && <Alert severity="error">{(error as Error).message}</Alert>}
      <DataGrid
        autoHeight
        rows={rows}
        columns={columns}
        pageSizeOptions={[10, 25, 50]}
        initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
        getRowClassName={({ row }) => (row.isOverdue ? 'ppe-row-overdue' : '')}
        sx={{
          '& .ppe-row-overdue': { bgcolor: 'error.light', '&:hover': { bgcolor: 'error.main', color: 'white' } },
        }}
        disableRowSelectionOnClick
      />
    </Box>
  );
}
