import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Grid,
  TextField,
  Typography,
} from '@mui/material';
import { workflowApi } from '../api/workflowApi';

export default function JsaReviewPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [rejectReason, setRejectReason] = useState('');

  const { data, isLoading, error } = useQuery({
    queryKey: ['jsa-review', id],
    queryFn: () => workflowApi.getJsaReview(id!),
    enabled: !!id,
  });

  const approveMutation = useMutation({
    mutationFn: () => workflowApi.approveJsa(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['jsa-review'] });
      queryClient.invalidateQueries({ queryKey: ['pending-actions'] });
      navigate('/jsas');
    },
  });

  const rejectMutation = useMutation({
    mutationFn: () => workflowApi.rejectJsa(id!, rejectReason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['jsa-review'] });
      navigate('/jsas');
    },
  });

  if (isLoading) return <CircularProgress />;
  if (error) return <Alert severity="error">{(error as Error).message}</Alert>;
  if (!data) return null;

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Supervisor Review</Typography>
      <Chip label={data.status} sx={{ mb: 2 }} />
      <Grid container spacing={2}>
        <Grid item xs={12} md={8}>
          <Card sx={{ mb: 2 }}>
            <CardContent>
              <Typography variant="h6">{data.title}</Typography>
              <Typography color="text.secondary" gutterBottom>{data.jobDescription}</Typography>
              <Typography><strong>Employee:</strong> {data.employeeName ?? '—'}</Typography>
              <Typography><strong>Department:</strong> {data.department}</Typography>
              <Typography><strong>Location:</strong> {data.location}</Typography>
              <Typography><strong>Section:</strong> {data.section}</Typography>
            </CardContent>
          </Card>
          <Card sx={{ mb: 2 }}>
            <CardContent>
              <Typography variant="subtitle1" gutterBottom>Hazards</Typography>
              {data.hazards.map((h, i) => (
                <Typography key={i}>• {h.description}</Typography>
              ))}
              <Typography variant="subtitle1" sx={{ mt: 2 }} gutterBottom>Controls</Typography>
              {data.controlMeasures.map((c, i) => (
                <Typography key={i}>• {c.description}</Typography>
              ))}
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" gutterBottom>Signature</Typography>
              <Typography>{data.signOffName} {data.signOffSurname}</Typography>
              <Typography variant="body2">{data.signOffOccupation} · {data.signOffCompanyNumber}</Typography>
              {data.status === 'Submitted' && (
                <Box sx={{ mt: 3, display: 'flex', flexDirection: 'column', gap: 2 }}>
                  <Button
                    variant="contained"
                    color="success"
                    disabled={approveMutation.isPending}
                    onClick={() => approveMutation.mutate()}
                  >
                    Approve
                  </Button>
                  <TextField
                    label="Rejection reason"
                    required
                    multiline
                    rows={3}
                    value={rejectReason}
                    onChange={(e) => setRejectReason(e.target.value)}
                  />
                  <Button
                    variant="outlined"
                    color="error"
                    disabled={!rejectReason.trim() || rejectMutation.isPending}
                    onClick={() => rejectMutation.mutate()}
                  >
                    Reject
                  </Button>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
