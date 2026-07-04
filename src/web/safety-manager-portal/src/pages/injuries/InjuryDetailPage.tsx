import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  CircularProgress,
  Alert,
  Button,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { safetyApi } from '../../api/safetyApi';

export default function InjuryDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data, isLoading, error } = useQuery({
    queryKey: ['injury', id],
    queryFn: () => safetyApi.getInjuryById(id!),
    enabled: !!id,
  });

  if (isLoading) return <CircularProgress />;
  if (error) return <Alert severity="error">{(error as Error).message}</Alert>;
  if (!data) return null;

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/injuries')} sx={{ mb: 2 }}>
        Back
      </Button>
      <Typography variant="h4" gutterBottom>{String(data.employeeName)}</Typography>
      <Card>
        <CardContent>
          <Grid container spacing={2}>
            {[
              ['Type', data.injuryType],
              ['Body Part', data.bodyPartInjured],
              ['Department', data.department],
              ['Location', data.location],
              ['Section', data.section],
              ['Status', data.status],
            ].map(([label, value]) => (
              <Grid item xs={12} sm={6} key={String(label)}>
                <Typography variant="caption" color="text.secondary">{String(label)}</Typography>
                <Typography fontWeight={600}>{String(value ?? '—')}</Typography>
              </Grid>
            ))}
          </Grid>
          <Typography variant="h6" sx={{ mt: 3, mb: 1 }}>Incident Description</Typography>
          <Typography>{String(data.incidentDescription)}</Typography>
        </CardContent>
      </Card>
    </Box>
  );
}
