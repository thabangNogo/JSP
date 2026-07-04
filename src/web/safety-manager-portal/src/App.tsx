import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider, CssBaseline } from '@mui/material';
import theme from './theme/theme';
import { AuthProvider } from './auth/AuthProvider';
import ProtectedRoute from './auth/ProtectedRoute';
import AppLayout from './components/layout/AppLayout';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import EmployeesPage from './pages/EmployeesPage';
import EmployeeDetailPage from './pages/EmployeeDetailPage';
import PlaceholderPage from './pages/PlaceholderPage';
import JsaListPage from './pages/JsaListPage';
import JsaReviewPage from './pages/JsaReviewPage';
import NotificationsPage from './pages/NotificationsPage';
import NearMissListPage from './pages/NearMissListPage';
import StopUnsafeWorkListPage from './pages/StopUnsafeWorkListPage';
import InjuryListPage from './pages/injuries/InjuryListPage';
import InjuryDetailPage from './pages/injuries/InjuryDetailPage';
import InjuryFormPage from './pages/injuries/InjuryFormPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, refetchOnWindowFocus: false },
  },
});

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <AuthProvider>
          <BrowserRouter>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route
                element={
                  <ProtectedRoute>
                    <AppLayout />
                  </ProtectedRoute>
                }
              >
                <Route index element={<DashboardPage />} />
                <Route path="employees" element={<EmployeesPage />} />
                <Route path="employees/:id" element={<EmployeeDetailPage />} />
                <Route path="jsas" element={<JsaListPage />} />
                <Route path="jsas/:id/review" element={<JsaReviewPage />} />
                <Route path="notifications" element={<NotificationsPage />} />
                <Route path="near-misses" element={<NearMissListPage />} />
                <Route path="injuries" element={<InjuryListPage />} />
                <Route path="injuries/new" element={<InjuryFormPage />} />
                <Route path="injuries/:id" element={<InjuryDetailPage />} />
                <Route path="stop-unsafe-work" element={<StopUnsafeWorkListPage />} />
                <Route path="corrective-actions" element={<PlaceholderPage title="Corrective Actions" />} />
                <Route path="reports" element={<PlaceholderPage title="Reports" />} />
                <Route path="settings" element={<PlaceholderPage title="Settings" />} />
              </Route>
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </BrowserRouter>
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}
