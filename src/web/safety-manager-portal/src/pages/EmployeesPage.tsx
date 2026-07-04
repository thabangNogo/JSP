import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  TextField,
  MenuItem,
  Typography,
  Chip,
  CircularProgress,
  IconButton,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { employeesApi } from '../api/employeesApi';
import { safetyApi } from '../api/safetyApi';
import { useAuth } from '../auth/AuthProvider';
import { DEPARTMENTS, OCCUPATIONS } from '../constants/filters';
import type { EmployeeListItem } from '../types/employee';

function StatCard({ title, value }: { title: string; value: number | string }) {
  return (
    <Card>
      <CardContent>
        <Typography variant="body2" color="text.secondary">
          {title}
        </Typography>
        <Typography variant="h5" fontWeight={700}>
          {value}
        </Typography>
      </CardContent>
    </Card>
  );
}

export default function EmployeesPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const isAdmin = user?.roles.includes('Administrator') ?? false;
  const [search, setSearch] = useState('');
  const [department, setDepartment] = useState('All Departments');
  const [occupation, setOccupation] = useState('All Occupations');
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 });
  const [addOpen, setAddOpen] = useState(false);
  const [addForm, setAddForm] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    employeeNumber: '',
    workDepartmentId: '',
    companyNumber: '',
    occupation: 'Operator',
  });
  const [addError, setAddError] = useState('');

  const { data: workLookups } = useQuery({
    queryKey: ['work-lookups'],
    queryFn: () => safetyApi.getWorkLookups(),
    enabled: isAdmin,
  });

  const createMutation = useMutation({
    mutationFn: () =>
      employeesApi.create({
        ...addForm,
        roles: ['Operator'],
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['employees'] });
      queryClient.invalidateQueries({ queryKey: ['employee-stats'] });
      setAddOpen(false);
      setAddForm({
        email: '',
        password: '',
        firstName: '',
        lastName: '',
        employeeNumber: '',
        workDepartmentId: '',
        companyNumber: '',
        occupation: 'Operator',
      });
      setAddError('');
    },
    onError: (err) => setAddError((err as Error).message),
  });

  const { data: stats } = useQuery({
    queryKey: ['employee-stats'],
    queryFn: () => employeesApi.getStats(),
  });

  const { data, isLoading, error } = useQuery({
    queryKey: ['employees', search, department, occupation, paginationModel],
    queryFn: () =>
      employeesApi.search({
        search: search || undefined,
        department: department !== 'All Departments' ? department : undefined,
        occupation: occupation !== 'All Occupations' ? occupation : undefined,
        page: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
      }),
  });

  const columns: GridColDef<EmployeeListItem>[] = useMemo(
    () => [
      { field: 'employeeNumber', headerName: 'Employee Number', flex: 1, minWidth: 140 },
      { field: 'name', headerName: 'Name', flex: 1, minWidth: 120 },
      { field: 'surname', headerName: 'Surname', flex: 1, minWidth: 120 },
      { field: 'department', headerName: 'Department', flex: 1.2, minWidth: 180 },
      { field: 'occupation', headerName: 'Occupation', flex: 1, minWidth: 120 },
      { field: 'assessmentsCompleted', headerName: 'Assessments', width: 120, type: 'number' },
      { field: 'nearMissesSubmitted', headerName: 'Near Misses', width: 110, type: 'number' },
      {
        field: 'status',
        headerName: 'Status',
        width: 110,
        renderCell: (params) => (
          <Chip
            label={params.value}
            size="small"
            color={params.value === 'Active' ? 'success' : 'default'}
          />
        ),
      },
      {
        field: 'actions',
        headerName: 'Actions',
        width: 90,
        sortable: false,
        renderCell: (params) => (
          <IconButton size="small" onClick={() => navigate(`/employees/${params.row.id}`)}>
            <VisibilityIcon fontSize="small" />
          </IconButton>
        ),
      },
    ],
    [navigate],
  );

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h4">Employees</Typography>
        {isAdmin && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setAddOpen(true)}>
            Add Employee
          </Button>
        )}
      </Box>

      {stats && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={2.4}>
            <StatCard title="Total Employees" value={stats.totalEmployees} />
          </Grid>
          <Grid item xs={12} sm={6} md={2.4}>
            <StatCard title="Employees Online" value={stats.employeesOnline} />
          </Grid>
          <Grid item xs={12} sm={6} md={2.4}>
            <StatCard title="Assessments Today" value={stats.assessmentsCompletedToday} />
          </Grid>
          <Grid item xs={12} sm={6} md={2.4}>
            <StatCard title="Near Misses This Month" value={stats.nearMissesThisMonth} />
          </Grid>
          <Grid item xs={12} sm={6} md={2.4}>
            <StatCard title="Avg Assessments / Employee" value={stats.averageAssessmentsPerEmployee} />
          </Grid>
        </Grid>
      )}

      <Card sx={{ mb: 2, p: 2 }}>
        <Grid container spacing={2}>
          <Grid item xs={12} md={4}>
            <TextField
              fullWidth
              label="Search Employee"
              placeholder="Name, surname, or company number"
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPaginationModel((p) => ({ ...p, page: 0 }));
              }}
              size="small"
            />
          </Grid>
          <Grid item xs={12} md={4}>
            <TextField
              fullWidth
              select
              label="Department"
              value={department}
              onChange={(e) => {
                setDepartment(e.target.value);
                setPaginationModel((p) => ({ ...p, page: 0 }));
              }}
              size="small"
            >
              {DEPARTMENTS.map((d) => (
                <MenuItem key={d} value={d}>
                  {d}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid item xs={12} md={4}>
            <TextField
              fullWidth
              select
              label="Occupation"
              value={occupation}
              onChange={(e) => {
                setOccupation(e.target.value);
                setPaginationModel((p) => ({ ...p, page: 0 }));
              }}
              size="small"
            >
              {OCCUPATIONS.map((o) => (
                <MenuItem key={o} value={o}>
                  {o}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
        </Grid>
      </Card>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{(error as Error).message}</Alert>}

      <Card>
        <DataGrid
          rows={data?.items ?? []}
          columns={columns}
          loading={isLoading}
          rowCount={data?.totalCount ?? 0}
          paginationMode="server"
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          pageSizeOptions={[10, 20, 50]}
          autoHeight
          disableRowSelectionOnClick
          onRowClick={(params) => navigate(`/employees/${params.id}`)}
          sx={{ border: 0, minHeight: 400 }}
        />
      </Card>

      <Dialog open={addOpen} onClose={() => setAddOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Employee</DialogTitle>
        <DialogContent>
          {addError && <Alert severity="error" sx={{ mb: 2 }}>{addError}</Alert>}
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                required
                label="First Name"
                value={addForm.firstName}
                onChange={(e) => setAddForm((f) => ({ ...f, firstName: e.target.value }))}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                required
                label="Last Name"
                value={addForm.lastName}
                onChange={(e) => setAddForm((f) => ({ ...f, lastName: e.target.value }))}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                required
                type="email"
                label="Email"
                value={addForm.email}
                onChange={(e) => setAddForm((f) => ({ ...f, email: e.target.value }))}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                required
                type="password"
                label="Password"
                value={addForm.password}
                onChange={(e) => setAddForm((f) => ({ ...f, password: e.target.value }))}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                required
                label="Employee Number"
                value={addForm.employeeNumber}
                onChange={(e) => setAddForm((f) => ({ ...f, employeeNumber: e.target.value }))}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                required
                label="Company Number"
                value={addForm.companyNumber}
                onChange={(e) => setAddForm((f) => ({ ...f, companyNumber: e.target.value }))}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                required
                select
                label="Department"
                value={addForm.workDepartmentId}
                onChange={(e) => setAddForm((f) => ({ ...f, workDepartmentId: e.target.value }))}
              >
                {(workLookups?.departments ?? []).map((d) => (
                  <MenuItem key={d.id} value={d.id}>{d.name}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                required
                select
                label="Occupation"
                value={addForm.occupation}
                onChange={(e) => setAddForm((f) => ({ ...f, occupation: e.target.value }))}
              >
                {OCCUPATIONS.filter((o) => o !== 'All Occupations').map((o) => (
                  <MenuItem key={o} value={o}>{o}</MenuItem>
                ))}
              </TextField>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAddOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            disabled={createMutation.isPending}
            onClick={() => createMutation.mutate()}
          >
            Create Employee
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
