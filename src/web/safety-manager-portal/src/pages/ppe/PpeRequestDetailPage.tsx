import { useNavigate, useParams } from 'react-router-dom';
import {
  Box, Typography, Button, CircularProgress, Alert, Card, CardContent, Grid, Chip, Stepper, Step, StepLabel,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { usePpeRequest } from '../../hooks/usePpe';
import PpeRequestWorkflowActions from '../../components/ppe/PpeRequestWorkflowActions';

export default function PpeRequestDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data, isLoading, error, refetch } = usePpeRequest(id);

  if (isLoading) return <CircularProgress />;
  if (error) return <Alert severity="error">{(error as Error).message}</Alert>;
  if (!data) return null;

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/ppe/requests')} sx={{ mb: 2 }}>Back</Button>
      <Box display="flex" alignItems="center" gap={2} mb={2}>
        <Typography variant="h4">{data.requestNumber}</Typography>
        <Chip label={data.status} color={data.isOverdue ? 'error' : 'primary'} />
        {data.isOverdue && <Chip label="Overdue" color="error" variant="outlined" />}
      </Box>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <PpeRequestWorkflowActions
            requestId={data.id}
            status={data.status}
            employeeName={data.employeeName}
            onActionComplete={() => refetch()}
          />
        </CardContent>
      </Card>

      <Grid container spacing={2}>
        <Grid item xs={12} md={6}>
          <Card><CardContent>
            <Typography variant="h6" mb={1}>Request Details</Typography>
            <Detail label="Employee" value={data.employeeName} />
            <Detail label="Department" value={data.department} />
            <Detail label="Location" value={data.location} />
            <Detail label="Section" value={data.section} />
            <Detail label="PPE Item" value={data.ppeItemName} />
            <Detail label="Quantity" value={String(data.quantity)} />
            <Detail label="Priority" value={data.priority} />
            <Detail label="Reason" value={data.reason} />
            {data.comments && <Detail label="Comments" value={data.comments} />}
            {data.dispatchDate && <Detail label="Dispatch Date" value={new Date(data.dispatchDate).toLocaleString()} />}
            {data.collectedDate && <Detail label="Collected Date" value={new Date(data.collectedDate).toLocaleDateString()} />}
          </CardContent></Card>
        </Grid>
        <Grid item xs={12} md={6}>
          <Card><CardContent>
            <Typography variant="h6" mb={2}>History Timeline</Typography>
            <Stepper orientation="vertical" activeStep={data.statusHistory.length}>
              {data.statusHistory.map((h) => (
                <Step key={h.id} completed>
                  <StepLabel>
                    {h.newStatus} · {new Date(h.actionDate).toLocaleString()}
                    <Typography variant="caption" display="block">{h.actionByUserName}</Typography>
                    {h.comments && (
                      <Typography variant="caption" display="block" color="text.secondary">{h.comments}</Typography>
                    )}
                  </StepLabel>
                </Step>
              ))}
            </Stepper>
          </CardContent></Card>
        </Grid>
      </Grid>
    </Box>
  );
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <Box mb={1}>
      <Typography variant="caption" color="text.secondary">{label}</Typography>
      <Typography>{value}</Typography>
    </Box>
  );
}
