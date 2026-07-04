import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
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
import { apiClient, unwrap } from '../../api/axiosClient';
import { employeesApi } from '../../api/employeesApi';
import { safetyApi } from '../../api/safetyApi';
import type { EmployeeListItem } from '../../types/employee';

const injuryTypes = ['FirstAidInjury', 'MedicalTreatmentInjury', 'LostTimeInjury', 'Fatality'];
const bodyParts = [
  'Head', 'Eye', 'Face', 'Neck', 'Shoulder', 'Arm', 'Hand', 'Finger',
  'Chest', 'Back', 'Leg', 'Knee', 'Foot', 'Multiple', 'Other',
];

export default function InjuryFormPage() {
  const navigate = useNavigate();
  const [employeeSearch, setEmployeeSearch] = useState('');
  const [selectedEmployee, setSelectedEmployee] = useState<EmployeeListItem | null>(null);
  const [form, setForm] = useState({
    department: '',
    location: '',
    section: '',
    injuryOccurredAt: new Date().toISOString().slice(0, 16),
    injuryType: 'FirstAidInjury',
    bodyPartInjured: 'Hand',
    incidentDescription: '',
    status: 'Open',
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

  const employeeOptions = useMemo(
    () => employeeResults?.items ?? [],
    [employeeResults],
  );

  const mutation = useMutation({
    mutationFn: (payload: {
      employeeUserId?: string;
      employeeName: string;
      department: string;
      location: string;
      section: string;
      injuryOccurredAt: string;
      injuryType: string;
      bodyPartInjured: string;
      incidentDescription: string;
      status: string;
    }) =>
      unwrap<{ id: string }>(
        apiClient.post('/injuries', {
          ...payload,
          injuryOccurredAt: new Date(payload.injuryOccurredAt).toISOString(),
        }),
      ),
    onSuccess: (data) => navigate(`/injuries/${data.id}`),
  });

  const update = (key: string, value: string) => setForm((f) => ({ ...f, [key]: value }));

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedEmployee) return;
    mutation.mutate({
      employeeUserId: selectedEmployee.id,
      employeeName: `${selectedEmployee.name} ${selectedEmployee.surname}`.trim(),
      ...form,
    });
  };

  const handleEmployeeChange = (_: unknown, value: EmployeeListItem | null) => {
    setSelectedEmployee(value);
    if (value?.department) {
      setForm((f) => ({ ...f, department: value.department }));
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Capture Injury</Typography>
      {mutation.isError && (
        <Alert severity="error" sx={{ mb: 2 }}>{(mutation.error as Error).message}</Alert>
      )}
      <Card>
        <CardContent>
          {lookupsLoading ? (
            <CircularProgress />
          ) : (
            <Grid container spacing={2} component="form" onSubmit={handleSubmit}>
              <Grid item xs={12} md={6}>
                <Autocomplete
                  options={employeeOptions}
                  loading={employeesLoading}
                  value={selectedEmployee}
                  onChange={handleEmployeeChange}
                  onInputChange={(_, value) => setEmployeeSearch(value)}
                  getOptionLabel={(o) => `${o.name} ${o.surname} (${o.employeeNumber})`}
                  isOptionEqualToValue={(a, b) => a.id === b.id}
                  noOptionsText={employeeSearch.length < 2 ? 'Type at least 2 characters' : 'No employees found'}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      required
                      label="Employee"
                      placeholder="Search by name or employee number"
                    />
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
                <TextField
                  fullWidth
                  required
                  type="datetime-local"
                  label="Date & Time"
                  InputLabelProps={{ shrink: true }}
                  value={form.injuryOccurredAt}
                  onChange={(e) => update('injuryOccurredAt', e.target.value)}
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <TextField
                  fullWidth
                  select
                  label="Injury Type"
                  value={form.injuryType}
                  onChange={(e) => update('injuryType', e.target.value)}
                >
                  {injuryTypes.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                </TextField>
              </Grid>
              <Grid item xs={12} md={3}>
                <TextField
                  fullWidth
                  select
                  label="Body Part"
                  value={form.bodyPartInjured}
                  onChange={(e) => update('bodyPartInjured', e.target.value)}
                >
                  {bodyParts.map((b) => <MenuItem key={b} value={b}>{b}</MenuItem>)}
                </TextField>
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  required
                  multiline
                  rows={4}
                  label="Incident Description"
                  value={form.incidentDescription}
                  onChange={(e) => update('incidentDescription', e.target.value)}
                />
              </Grid>
              <Grid item xs={12}>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={mutation.isPending || !selectedEmployee}
                >
                  Submit Injury
                </Button>
              </Grid>
            </Grid>
          )}
        </CardContent>
      </Card>
    </Box>
  );
}
