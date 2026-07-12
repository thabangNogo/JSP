import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Typography, Button, CircularProgress, Alert, Chip, Dialog, DialogTitle, DialogContent,
  DialogActions, TextField, MenuItem,
} from '@mui/material';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import AddIcon from '@mui/icons-material/Add';
import { useAuth } from '../../auth/AuthProvider';
import { usePpeMutations, usePpeRequests } from '../../hooks/usePpe';
import type { PpeRequestListItem } from '../../types/ppe';

export default function PpeRequestListPage({ archivedOnly = false, issuedOnly = false }: { archivedOnly?: boolean; issuedOnly?: boolean }) {
  const navigate = useNavigate();
  const { user } = useAuth();
  const filter = archivedOnly ? { archivedOnly: true } : issuedOnly ? { issuedOnly: true } : { archivedOnly: false };
  const { data, isLoading, error } = usePpeRequests(filter);
  const { approveRequest, archiveRequest, dispatchRequest } = usePpeMutations();
  const [dispatchRow, setDispatchRow] = useState<PpeRequestListItem | null>(null);
  const [dispatchForm, setDispatchForm] = useState({
    dispatchDate: new Date().toISOString().slice(0, 10),
    collectedByEmployee: '',
    employeeSignature: '',
    safetyOfficerSignature: '',
    notes: '',
  });

  const openDispatch = (row: PpeRequestListItem) => {
    setDispatchForm({
      dispatchDate: new Date().toISOString().slice(0, 10),
      collectedByEmployee: row.employeeName,
      employeeSignature: '',
      safetyOfficerSignature: user ? `${user.firstName} ${user.lastName}`.trim() : '',
      notes: '',
    });
    setDispatchRow(row);
  };

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
      width: 280,
      sortable: false,
      renderCell: ({ row }) => (
        <Box display="flex" gap={0.5} flexWrap="wrap">
          <Button size="small" onClick={() => navigate(`/ppe/requests/${row.id}`)}>View</Button>
          {!archivedOnly && row.status === 'Requested' && (
            <Button size="small" onClick={() => navigate(`/ppe/requests/${row.id}/edit`)}>Edit</Button>
          )}
          {row.status === 'PendingApproval' && (
            <Button size="small" onClick={() => approveRequest.mutate(String(row.id))}>Approve</Button>
          )}
          {row.status === 'Preparing' && (
            <Button size="small" onClick={() => openDispatch(row as PpeRequestListItem)}>Dispatch</Button>
          )}
          {(row.status === 'Completed' || row.status === 'Rejected') && (
            <Button size="small" onClick={() => archiveRequest.mutate(String(row.id))}>Archive</Button>
          )}
        </Box>
      ),
    },
  ], [approveRequest, archiveRequest, archivedOnly, navigate, user]);

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

      <Dialog open={Boolean(dispatchRow)} onClose={() => setDispatchRow(null)} maxWidth="sm" fullWidth>
        <DialogTitle>Dispatch PPE</DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2, pt: 1 }}>
          <TextField
            label="Issued By"
            value={user ? `${user.firstName} ${user.lastName}`.trim() : ''}
            InputProps={{ readOnly: true }}
          />
          <TextField
            label="Dispatch Date"
            type="date"
            InputLabelProps={{ shrink: true }}
            value={dispatchForm.dispatchDate}
            onChange={(e) => setDispatchForm((f) => ({ ...f, dispatchDate: e.target.value }))}
          />
          <TextField
            label="Collected By Employee"
            value={dispatchForm.collectedByEmployee}
            onChange={(e) => setDispatchForm((f) => ({ ...f, collectedByEmployee: e.target.value }))}
          />
          <TextField
            label="Employee Signature"
            placeholder="Employee full name or signature reference"
            value={dispatchForm.employeeSignature}
            onChange={(e) => setDispatchForm((f) => ({ ...f, employeeSignature: e.target.value }))}
          />
          <TextField
            label="Safety Officer Signature"
            placeholder="Officer full name or signature reference"
            value={dispatchForm.safetyOfficerSignature}
            onChange={(e) => setDispatchForm((f) => ({ ...f, safetyOfficerSignature: e.target.value }))}
          />
          <TextField
            label="Notes"
            multiline
            minRows={2}
            value={dispatchForm.notes}
            onChange={(e) => setDispatchForm((f) => ({ ...f, notes: e.target.value }))}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDispatchRow(null)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={() => {
              if (!dispatchRow) return;
              dispatchRequest.mutate({
                id: dispatchRow.id,
                payload: {
                  dispatchDate: dispatchForm.dispatchDate,
                  collectedByEmployee: dispatchForm.collectedByEmployee,
                  employeeSignature: dispatchForm.employeeSignature || null,
                  safetyOfficerSignature: dispatchForm.safetyOfficerSignature || null,
                  notes: dispatchForm.notes,
                },
              });
              setDispatchRow(null);
            }}
          >
            Dispatch
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
