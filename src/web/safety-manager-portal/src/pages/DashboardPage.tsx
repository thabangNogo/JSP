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
import { usePpeDashboardKpis, usePpeRequests } from '../hooks/usePpe';
import { useNavigate } from 'react-router-dom';

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
  const navigate = useNavigate();
  const { data: ppeKpis } = usePpeDashboardKpis();
  const { data: overduePpe } = usePpeRequests({ archivedOnly: false });

  const overdueRows = (overduePpe ?? []).filter((r) => r.isOverdue).slice(0, 5);

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

      {ppeKpis && (
        <Card sx={{ mb: 3, borderLeft: 4, borderColor: ppeKpis.overdueRequests > 0 ? 'error.main' : 'primary.main' }}>
          <CardContent>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
              <Typography variant="h6">Overdue PPE Requests</Typography>
              <Typography
                component="button"
                variant="body2"
                onClick={() => navigate('/ppe')}
                sx={{ border: 0, bgcolor: 'transparent', cursor: 'pointer', color: 'primary.main' }}
              >
                Open PPE Dashboard
              </Typography>
            </Box>
            <Grid container spacing={2} sx={{ mb: overdueRows.length ? 2 : 0 }}>
              <Grid item xs={6} sm={4} md={2}>
                <StatCard title="Open Requests" value={ppeKpis.openRequests} />
              </Grid>
              <Grid item xs={6} sm={4} md={2}>
                <StatCard title="Pending Approval" value={ppeKpis.pendingApproval} />
              </Grid>
              <Grid item xs={6} sm={4} md={2}>
                <StatCard title="Overdue" value={ppeKpis.overdueRequests} />
              </Grid>
              <Grid item xs={6} sm={4} md={2}>
                <StatCard title="Low Stock Items" value={ppeKpis.lowStockItems} />
              </Grid>
            </Grid>
            {overdueRows.length > 0 && (
              <Box>
                {overdueRows.map((r) => (
                  <Box
                    key={r.id}
                    sx={{
                      py: 1,
                      px: 1.5,
                      mb: 0.5,
                      borderRadius: 1,
                      bgcolor: 'error.light',
                      display: 'flex',
                      justifyContent: 'space-between',
                      cursor: 'pointer',
                    }}
                    onClick={() => navigate(`/ppe/requests/${r.id}`)}
                  >
                    <Typography variant="body2">{r.requestNumber} · {r.employeeName} · {r.ppeItemName}</Typography>
                    <Typography variant="caption">{r.ageDays} days</Typography>
                  </Box>
                ))}
              </Box>
            )}
          </CardContent>
        </Card>
      )}

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
