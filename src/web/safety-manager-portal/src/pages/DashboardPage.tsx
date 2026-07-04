import { useQuery } from '@tanstack/react-query';
import { Box, Card, CardContent, Grid, Typography, CircularProgress, Alert } from '@mui/material';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { safetyApi } from '../api/safetyApi';
import { workflowApi } from '../api/workflowApi';

function StatCard({ title, value }: { title: string; value: number | string }) {
  return (
    <Card>
      <CardContent>
        <Typography variant="body2" color="text.secondary" gutterBottom>
          {title}
        </Typography>
        <Typography variant="h4" fontWeight={700}>
          {value}
        </Typography>
      </CardContent>
    </Card>
  );
}

export default function DashboardPage() {
  const { data: pending } = useQuery({
    queryKey: ['pending-actions'],
    queryFn: () => workflowApi.getPendingActions(),
  });

  const { data: kpis, isLoading, error } = useQuery({
    queryKey: ['manager-kpis'],
    queryFn: async () => {
      const [manager, ifd] = await Promise.all([
        safetyApi.getManagerKpis(),
        safetyApi.getInjuryFreeDays(),
      ]);
      return { ...manager, injuryFreeDays: ifd.injuryFreeDays };
    },
  });

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" py={8}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{(error as Error).message}</Alert>;
  }

  if (!kpis) return null;

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>

      <Card
        sx={{
          mb: 3,
          background: 'linear-gradient(135deg, #2e7d32, #43a047)',
          color: 'white',
        }}
      >
        <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 3, py: 4 }}>
          <Typography sx={{ fontSize: 56 }}>🦺</Typography>
          <Box>
            <Typography variant="overline" sx={{ letterSpacing: 1.5, opacity: 0.9 }}>
              INJURY FREE DAYS
            </Typography>
            <Typography variant="h2" fontWeight={800}>
              {kpis.injuryFreeDays}
            </Typography>
            <Typography fontStyle="italic">Keep it Safe!</Typography>
          </Box>
        </CardContent>
      </Card>

      {pending && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12}>
            <Typography variant="h6">Action Required</Typography>
          </Grid>
          <Grid item xs={12} sm={6} md={4} lg={2}>
            <StatCard title="Pending JSA Approvals" value={pending.pendingJsaApprovals} />
          </Grid>
          <Grid item xs={12} sm={6} md={4} lg={2}>
            <StatCard title="Near Misses to Investigate" value={pending.nearMissesAwaitingInvestigation} />
          </Grid>
          <Grid item xs={12} sm={6} md={4} lg={2}>
            <StatCard title="Overdue Corrective Actions" value={pending.correctiveActionsOverdue} />
          </Grid>
          <Grid item xs={12} sm={6} md={4} lg={2}>
            <StatCard title="Open Injuries" value={pending.openInjuries} />
          </Grid>
          <Grid item xs={12} sm={6} md={4} lg={2}>
            <StatCard title="Open Stop Work" value={pending.openUnsafeWorkReports} />
          </Grid>
          <Grid item xs={12} sm={6} md={4} lg={2}>
            <StatCard title="Draft JSAs" value={pending.pendingJsas} />
          </Grid>
        </Grid>
      )}

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <StatCard title="Injury Free Days" value={kpis.injuryFreeDays} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <StatCard title="Open Near Misses" value={kpis.openNearMisses} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <StatCard title="Open Corrective Actions" value={kpis.openCorrectiveActions} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <StatCard title="Assessments Submitted Today" value={kpis.assessmentsSubmittedToday} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <StatCard title="Total Employees" value={kpis.totalEmployees} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <StatCard title="Near Misses This Month" value={kpis.nearMissesThisMonth} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <StatCard title="Lost Time Injuries" value={kpis.lostTimeInjuries} />
        </Grid>
        <Grid item xs={12} sm={6} md={4} lg={3}>
          <StatCard title="First Aid Injuries" value={kpis.firstAidInjuries} />
        </Grid>
      </Grid>

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Near Miss Trend
          </Typography>
          <ResponsiveContainer width="100%" height={280}>
            <LineChart data={kpis.nearMissTrend}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="label" />
              <YAxis />
              <Tooltip />
              <Line type="monotone" dataKey="value" stroke="#1565c0" strokeWidth={2} />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>
    </Box>
  );
}
