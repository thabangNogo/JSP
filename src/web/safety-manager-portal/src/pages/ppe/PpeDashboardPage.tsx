import { Box, Card, CardContent, Grid, Typography, CircularProgress, Alert } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { usePpeDashboardKpis } from '../../hooks/usePpe';

function KpiCard({ label, value, color, onClick }: { label: string; value: number; color?: string; onClick?: () => void }) {
  return (
    <Card sx={{ cursor: onClick ? 'pointer' : 'default', borderLeft: color ? `4px solid ${color}` : undefined }} onClick={onClick}>
      <CardContent>
        <Typography variant="body2" color="text.secondary">{label}</Typography>
        <Typography variant="h4" fontWeight={800}>{value}</Typography>
      </CardContent>
    </Card>
  );
}

export default function PpeDashboardPage() {
  const navigate = useNavigate();
  const { data, isLoading, error } = usePpeDashboardKpis();

  if (isLoading) return <CircularProgress />;
  if (error) return <Alert severity="error">{(error as Error).message}</Alert>;
  if (!data) return null;

  return (
    <Box>
      <Typography variant="h4" mb={2}>PPE Dashboard</Typography>
      <Grid container spacing={2}>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <KpiCard label="Open PPE Requests" value={data.openRequests} onClick={() => navigate('/ppe/requests')} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <KpiCard label="Pending Approval" value={data.pendingApproval} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <KpiCard label="Preparing" value={data.preparing} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <KpiCard label="Ready For Collection" value={data.readyForCollection} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <KpiCard label="Collected Today" value={data.collectedToday} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <KpiCard label="Overdue PPE Requests" value={data.overdueRequests} color="#d32f2f" onClick={() => navigate('/ppe/reports?tab=overdue')} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <KpiCard label="Low Stock PPE" value={data.lowStockItems} color="#ed6c02" onClick={() => navigate('/ppe/catalogue')} />
        </Grid>
      </Grid>
    </Box>
  );
}
