import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Alert,
  Autocomplete,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Grid,
  MenuItem,
  TextField,
  Typography,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { useQuery } from '@tanstack/react-query';
import { employeesApi } from '../../api/employeesApi';
import { safetyApi } from '../../api/safetyApi';
import { usePpeCatalogue, usePpeMutations, usePpeRequest } from '../../hooks/usePpe';
import type { EmployeeListItem } from '../../types/employee';
import type { PpeCatalogueItem, PpeRequestPriority } from '../../types/ppe';

const priorities: PpeRequestPriority[] = ['Low', 'Medium', 'High', 'Urgent'];

export default function PpeRequestFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEdit = Boolean(id);
  const navigate = useNavigate();
  const { createRequest, updateRequest } = usePpeMutations();
  const { data: existing, isLoading: loadingExisting } = usePpeRequest(isEdit ? id : undefined);
  const { data: catalogue, isLoading: catalogueLoading } = usePpeCatalogue(true);

  const [employeeSearch, setEmployeeSearch] = useState('');
  const [selectedEmployee, setSelectedEmployee] = useState<EmployeeListItem | null>(null);
  const [selectedItem, setSelectedItem] = useState<PpeCatalogueItem | null>(null);
  const [form, setForm] = useState({
    department: '',
    location: '',
    section: '',
    quantity: 1,
    reason: '',
    priority: 'Medium' as PpeRequestPriority,
    requiredByDate: new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 10),
    comments: '',
  });

  const { data: lookups, isLoading: lookupsLoading } = useQuery({
    queryKey: ['work-lookups'],
    queryFn: () => safetyApi.getWorkLookups(),
  });

  const { data: employeeResults, isFetching: employeesLoading } = useQuery({
    queryKey: ['employee-search', employeeSearch],
    queryFn: () =>
      employeesApi.search({
        search: employeeSearch || undefined,
        page: 1,
        pageSize: 20,
      }),
    enabled: employeeSearch.length >= 2,
  });

  const employeeOptions = useMemo(() => employeeResults?.items ?? [], [employeeResults]);

  useEffect(() => {
    if (!existing || !isEdit) return;
    setForm({
      department: existing.department,
      location: existing.location,
      section: existing.section,
      quantity: existing.quantity,
      reason: existing.reason,
      priority: existing.priority,
      requiredByDate: existing.requiredByDate.slice(0, 10),
      comments: existing.comments ?? '',
    });
    setSelectedEmployee({
      id: existing.employeeUserId,
      name: existing.employeeName.split(' ')[0] ?? existing.employeeName,
      surname: existing.employeeName.split(' ').slice(1).join(' ') || '',
      employeeNumber: '',
      department: existing.department,
      occupation: '',
      assessmentsCompleted: 0,
      nearMissesSubmitted: 0,
      status: 'Active',
      isActive: true,
    });
    if (catalogue) {
      const item = catalogue.find((c) => c.id === existing.ppeCatalogueItemId);
      if (item) setSelectedItem(item);
    }
  }, [existing, isEdit, catalogue]);

  const update = (key: string, value: string | number) =>
    setForm((f) => ({ ...f, [key]: value }));

  const workDepartmentId = useMemo(() => {
    const match = lookups?.departments.find((d) => d.name === form.department);
    return match?.id;
  }, [lookups, form.department]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedEmployee || !selectedItem) return;

    const payload = {
      employeeUserId: selectedEmployee.id,
      workDepartmentId: workDepartmentId ?? null,
      department: form.department,
      location: form.location,
      section: form.section,
      ppeCatalogueItemId: selectedItem.id,
      quantity: Number(form.quantity),
      reason: form.reason,
      priority: form.priority,
      requiredByDate: new Date(form.requiredByDate).toISOString(),
      comments: form.comments || null,
    };

    if (isEdit && id) {
      updateRequest.mutate(
        { id, payload },
        { onSuccess: (data) => navigate(`/ppe/requests/${data.id}`) },
      );
    } else {
      createRequest.mutate(payload, {
        onSuccess: (data) => navigate(`/ppe/requests/${data.id}`),
      });
    }
  };

  const pending = createRequest.isPending || updateRequest.isPending;
  const error = createRequest.error ?? updateRequest.error;
  const loading = lookupsLoading || catalogueLoading || (isEdit && loadingExisting);

  if (loading) return <CircularProgress />;
  if (isEdit && existing && existing.status !== 'Requested') {
    return (
      <Alert severity="warning">
        Only requests in Requested status can be edited.{' '}
        <Button onClick={() => navigate(`/ppe/requests/${id}`)}>View request</Button>
      </Alert>
    );
  }

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/ppe/requests')} sx={{ mb: 2 }}>
        Back
      </Button>
      <Typography variant="h4" gutterBottom>
        {isEdit ? 'Edit PPE Request' : 'New PPE Request'}
      </Typography>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{(error as Error).message}</Alert>}
      <Card>
        <CardContent>
          <Grid container spacing={2} component="form" onSubmit={handleSubmit}>
            <Grid item xs={12} md={6}>
              <Autocomplete
                options={employeeOptions}
                loading={employeesLoading}
                value={selectedEmployee}
                onChange={(_, value) => {
                  setSelectedEmployee(value);
                  if (value?.department) update('department', value.department);
                }}
                onInputChange={(_, value) => setEmployeeSearch(value)}
                getOptionLabel={(o) => `${o.name} ${o.surname} (${o.employeeNumber || o.id.slice(0, 8)})`}
                isOptionEqualToValue={(a, b) => a.id === b.id}
                disabled={isEdit}
                noOptionsText={employeeSearch.length < 2 ? 'Type at least 2 characters' : 'No employees found'}
                renderInput={(params) => (
                  <TextField {...params} required label="Employee" placeholder="Search by name or number" />
                )}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                required
                select
                label="Department"
                value={form.department}
                onChange={(e) => update('department', e.target.value)}
              >
                {(lookups?.departments ?? []).map((d) => (
                  <MenuItem key={d.id} value={d.name}>{d.name}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                required
                select
                label="Location"
                value={form.location}
                onChange={(e) => update('location', e.target.value)}
              >
                {(lookups?.locations ?? []).map((l) => (
                  <MenuItem key={l.id} value={l.name}>{l.name}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                required
                select
                label="Section"
                value={form.section}
                onChange={(e) => update('section', e.target.value)}
              >
                {(lookups?.sections ?? []).map((s) => (
                  <MenuItem key={s.id} value={s.name}>{s.name}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid item xs={12} md={6}>
              <Autocomplete
                options={catalogue ?? []}
                value={selectedItem}
                onChange={(_, value) => setSelectedItem(value)}
                getOptionLabel={(o) => `${o.itemName} (stock: ${o.quantityInStock})`}
                isOptionEqualToValue={(a, b) => a.id === b.id}
                renderInput={(params) => <TextField {...params} required label="PPE Item" />}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                required
                type="number"
                label="Quantity"
                inputProps={{ min: 1 }}
                value={form.quantity}
                onChange={(e) => update('quantity', e.target.value)}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                select
                label="Priority"
                value={form.priority}
                onChange={(e) => update('priority', e.target.value)}
              >
                {priorities.map((p) => <MenuItem key={p} value={p}>{p}</MenuItem>)}
              </TextField>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                required
                type="date"
                label="Required By Date"
                InputLabelProps={{ shrink: true }}
                value={form.requiredByDate}
                onChange={(e) => update('requiredByDate', e.target.value)}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                required
                multiline
                rows={3}
                label="Reason"
                value={form.reason}
                onChange={(e) => update('reason', e.target.value)}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                multiline
                rows={2}
                label="Comments"
                value={form.comments}
                onChange={(e) => update('comments', e.target.value)}
              />
            </Grid>
            <Grid item xs={12}>
              <Button type="submit" variant="contained" disabled={pending || !selectedEmployee || !selectedItem}>
                {isEdit ? 'Save Changes' : 'Create Request'}
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    </Box>
  );
}
